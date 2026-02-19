using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TimeCapital.Application.Common;
using TimeCapital.Data;
using TimeCapital.Domain.Entities;

namespace TimeCapital.Application.Sessions;

public sealed class SessionService : ISessionService
{
    private readonly ApplicationDbContext _db;

    public SessionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<StartSessionResponse> StartSessionAsync(string userId, StartSessionRequest req, CancellationToken ct = default)
    {
        if (req.ProjectId == Guid.Empty)
            throw new ValidationException("ProjectId inválido.");

        // validar Project + ownership
        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == req.ProjectId)
            .Select(p => new { p.Id, p.OwnerId })
            .SingleOrDefaultAsync(ct);

        if (project is null)
            throw new NotFoundException("Projeto não encontrado.");

        if (project.OwnerId != userId)
            throw new ForbiddenException("Projeto não pertence ao usuário.");

        // validar Goal (se enviado)
        if (req.GoalId is Guid goalId)
        {
            var goalOk = await _db.Goals
                .AsNoTracking()
                .AnyAsync(g => g.Id == goalId && g.ProjectId == req.ProjectId, ct);

            if (!goalOk)
                throw new ValidationException("Goal inválida para este projeto.");
        }

        // checagem amigável (índice filtrado garante concorrência)
        var hasActive = await _db.Sessions
            .AsNoTracking()
            .AnyAsync(s => s.UserId == userId && s.EndTimeUtc == null && s.CanceledAtUtc == null, ct);

        if (hasActive)
            throw new ConflictException("Já existe sessão ativa.");

        var now = DateTimeOffset.UtcNow;

        var session = new Session
        {
            UserId = userId,
            ProjectId = req.ProjectId,
            GoalId = req.GoalId,
            StartTimeUtc = now
        };

        _db.Sessions.Add(session);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new ConflictException("Já existe sessão ativa.");
        }

        return new StartSessionResponse(session.Id, session.StartTimeUtc);
    }

    public async Task<StopSessionResponse> StopSessionAsync(string userId, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Where(s => s.UserId == userId && s.EndTimeUtc == null && s.CanceledAtUtc == null)
            .SingleOrDefaultAsync(ct);

        if (session is null)
            throw new NotFoundException("Nenhuma sessão ativa encontrada.");

        var now = DateTimeOffset.UtcNow;

        session.EndTimeUtc = now;

        await _db.SaveChangesAsync(ct);

        var duration = (int)Math.Floor((now - session.StartTimeUtc).TotalSeconds);
        if (duration < 0) duration = 0;

        return new StopSessionResponse(session.Id, session.StartTimeUtc, now, duration);
    }

    public async Task<CancelSessionResponse> CancelActiveSessionAsync(string userId, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Where(s => s.UserId == userId && s.EndTimeUtc == null && s.CanceledAtUtc == null)
            .SingleOrDefaultAsync(ct);

        if (session is null)
            throw new NotFoundException("Nenhuma sessão ativa encontrada.");

        var now = DateTimeOffset.UtcNow;
        session.CanceledAtUtc = now;

        await _db.SaveChangesAsync(ct);

        return new CancelSessionResponse(session.Id, session.StartTimeUtc, now);
    }

public async Task<DashboardStateDto> GetDashboardStateAsync(string userId, CancellationToken ct = default)
{
    var now = DateTimeOffset.UtcNow;

    // "Hoje" em UTC (MVP)
    var todayStartUtc = new DateTimeOffset(now.Date, TimeSpan.Zero);

    // Semana (ISO-ish): segunda-feira 00:00 UTC
    var diff = ((int)now.DayOfWeek + 6) % 7; // Mon=0..Sun=6
    var weekStartUtc = new DateTimeOffset(now.Date.AddDays(-diff), TimeSpan.Zero);

    // defaultProjectId
    var defaultProjectId = await _db.Users
        .AsNoTracking()
        .Where(u => u.Id == userId)
        .Select(u => u.DefaultProjectId)
        .SingleAsync(ct);

    // projetos (dropdown)
    var projects = await _db.Projects
        .AsNoTracking()
        .Where(p => p.OwnerId == userId && p.Status == ProjectStatus.Active)
        .OrderBy(p => p.Title)
        .Select(p => new ProjectListItemDto(p.Id, p.Title))
        .ToListAsync(ct);

    // título do default (se houver)
    string? defaultProjectTitle = null;
    if (defaultProjectId is Guid dpId)
    {
        defaultProjectTitle = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == dpId && p.OwnerId == userId)
            .Select(p => p.Title)
            .SingleOrDefaultAsync(ct);
    }

    // sessão ativa
    var active = await _db.Sessions
        .AsNoTracking()
        .Where(s => s.UserId == userId && s.EndTimeUtc == null && s.CanceledAtUtc == null)
        .Select(s => new ActiveSessionDto(s.Id, s.ProjectId, s.GoalId, s.StartTimeUtc))
        .SingleOrDefaultAsync(ct);

    // meta (somente se sessão ativa tem GoalId)
    int? activeGoalTargetSeconds = null;
    if (active?.GoalId is Guid goalId)
    {
        activeGoalTargetSeconds = await _db.Goals
            .AsNoTracking()
            .Where(g => g.Id == goalId)
            .Select(g => g.TargetMinutes * 60)
            .SingleOrDefaultAsync(ct);

        if (activeGoalTargetSeconds == 0)
            activeGoalTargetSeconds = null;
    }

    // query base: sessões concluídas (não canceladas)
    var completed = _db.Sessions
        .AsNoTracking()
        .Where(s => s.UserId == userId && s.EndTimeUtc != null && s.CanceledAtUtc == null);

    // total do projeto default (somente concluídas)
    var defaultProjectTotalSeconds = 0;
    if (defaultProjectId is Guid dpId2)
    {
        defaultProjectTotalSeconds = await completed
            .Where(s => s.ProjectId == dpId2)
            .SumAsync(s => EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value), ct);
    }

    // totais hoje/semana (MVP simples: conta sessões que iniciaram dentro do período)
    var todayTotalSeconds = await completed
        .Where(s => s.StartTimeUtc >= todayStartUtc)
        .SumAsync(s => EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value), ct);

    var weekTotalSeconds = await completed
        .Where(s => s.StartTimeUtc >= weekStartUtc)
        .SumAsync(s => EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value), ct);

    // últimas sessões do projeto default
    var lastSessions = new List<LastSessionDto>();
    if (defaultProjectId is Guid dpId3)
    {
        lastSessions = await completed
            .Where(s => s.ProjectId == dpId3)
            .OrderByDescending(s => s.EndTimeUtc)
            .Take(10)
            .Join(_db.Projects.AsNoTracking(),
                  s => s.ProjectId,
                  p => p.Id,
                  (s, p) => new LastSessionDto(
                      s.Id,
                      s.ProjectId,
                      p.Title,
                      s.GoalId,
                      s.StartTimeUtc,
                      s.EndTimeUtc!.Value,
                      EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value)
                  ))
            .ToListAsync(ct);
    }

    // totals por projeto (mantém)
    var totalsByProject = await completed
        .GroupBy(s => s.ProjectId)
        .Select(g => new ProjectTotalDto(
            g.Key,
            g.Sum(x => EF.Functions.DateDiffSecond(x.StartTimeUtc, x.EndTimeUtc!.Value))
        ))
        .ToListAsync(ct);

    return new DashboardStateDto(
        defaultProjectId,
        defaultProjectTitle,
        projects,
        active,
        defaultProjectTotalSeconds,
        todayTotalSeconds,
        weekTotalSeconds,
        activeGoalTargetSeconds,
        lastSessions,
        totalsByProject
    );
}

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var sqlEx = ex.InnerException as SqlException
                    ?? ex.InnerException?.InnerException as SqlException;

        if (sqlEx is null) return false;

        return sqlEx.Number == 2601 || sqlEx.Number == 2627;
    }
}
