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
        var defaultProjectId = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.DefaultProjectId)
            .SingleAsync(ct);

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId && p.Status == ProjectStatus.Active)
            .OrderBy(p => p.Title)
            .Select(p => new ProjectListItemDto(p.Id, p.Title))
            .ToListAsync(ct);

        var active = await _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.EndTimeUtc == null && s.CanceledAtUtc == null)
            .Select(s => new ActiveSessionDto(s.Id, s.ProjectId, s.GoalId, s.StartTimeUtc))
            .SingleOrDefaultAsync(ct);

var totals = await _db.Sessions
    .AsNoTracking()
    .Where(s => s.UserId == userId && s.EndTimeUtc != null && s.CanceledAtUtc == null)
    .GroupBy(s => s.ProjectId)
    .Select(g => new ProjectTotalDto(
        g.Key,
        g.Sum(x => EF.Functions.DateDiffSecond(x.StartTimeUtc, x.EndTimeUtc!.Value))
    ))
    .ToListAsync(ct);


        return new DashboardStateDto(defaultProjectId, projects, active, totals);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var sqlEx = ex.InnerException as SqlException
                    ?? ex.InnerException?.InnerException as SqlException;

        if (sqlEx is null) return false;

        return sqlEx.Number == 2601 || sqlEx.Number == 2627;
    }
}
