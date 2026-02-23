using Microsoft.EntityFrameworkCore;
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

    // =========================
    // START
    // =========================
    public async Task<StartSessionResponse> StartSessionAsync(
        string userId,
        Guid projectId,
        Guid? goalId,
        CancellationToken ct = default)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId && p.OwnerId == userId)
            .SingleOrDefaultAsync(ct);

        if (project == null)
            throw new InvalidOperationException("Projeto inválido.");

        var hasActive = await _db.Sessions
            .AsNoTracking()
            .AnyAsync(s => s.UserId == userId &&
                           s.EndTimeUtc == null &&
                           s.CanceledAtUtc == null, ct);

        if (hasActive)
            throw new InvalidOperationException("Já existe sessão ativa.");

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProjectId = projectId,
            GoalId = goalId,
            StartTimeUtc = DateTimeOffset.UtcNow
        };

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(ct);

        return new StartSessionResponse(session.Id, session.StartTimeUtc);
    }

    // =========================
    // STOP
    // =========================
    public async Task<StopSessionResponse> StopSessionAsync(
        string userId,
        CancellationToken ct = default)
    {
        var active = await _db.Sessions
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc == null &&
                        s.CanceledAtUtc == null)
            .SingleOrDefaultAsync(ct);

        if (active == null)
            throw new InvalidOperationException("Nenhuma sessão ativa.");

         var endUtc = DateTimeOffset.UtcNow;
         active.EndTimeUtc = endUtc;
        await _db.SaveChangesAsync(ct);

        var durationSeconds = (int)Math.Max(0, (endUtc - active.StartTimeUtc).TotalSeconds);

        return new StopSessionResponse(
            active.Id,
            active.StartTimeUtc,
            active.EndTimeUtc!.Value,
            durationSeconds);
    }

public async Task DeleteSessionAsync(string userId, Guid sessionId, CancellationToken ct)
{
    // carrega sessão + projeto (pra validar dono/usuário)
    var session = await _db.Sessions
        .Include(s => s.Project)
        .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

    if (session is null)
        throw new InvalidOperationException("Sessão não encontrada.");

    // ✅ garante que a sessão é do usuário logado
    if (session.UserId != userId)
        throw new InvalidOperationException("Sessão inválida para este usuário.");

    // ✅ não deixar deletar sessão ativa (pra não bagunçar o timer)
    var isActive = session.EndTimeUtc == null && session.CanceledAtUtc == null;
    if (isActive)
        throw new InvalidOperationException("Não é possível excluir uma sessão ativa. Finalize ou cancele primeiro.");

    _db.Sessions.Remove(session);
    await _db.SaveChangesAsync(ct);
}


    // =========================
    // CANCEL
    // =========================
    public async Task CancelActiveSessionAsync(
        string userId,
        CancellationToken ct = default)
    {
        var active = await _db.Sessions
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc == null &&
                        s.CanceledAtUtc == null)
            .SingleOrDefaultAsync(ct);

        if (active == null)
            throw new InvalidOperationException("Nenhuma sessão ativa.");

        active.CanceledAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    // =========================
    // DASHBOARD
    // =========================
    public async Task<DashboardStateDto> GetDashboardStateAsync(
        string userId,
        Guid? selectedProjectId = null,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Início do dia/semana em UTC
        var todayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        var diff = ((int)now.DayOfWeek + 6) % 7; // Monday=0
        var weekStart = new DateTimeOffset(now.UtcDateTime.Date.AddDays(-diff), TimeSpan.Zero);

        // Preferência (pode ser null)
        var defaultProjectId = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.DefaultProjectId)
            .SingleAsync(ct);

        // Lista de projetos
        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .OrderBy(p => p.Title)
            .Select(p => new ProjectListItemDto(p.Id, p.Title))
            .ToListAsync(ct);

        // Projeto efetivo: query > default > primeiro projeto
        Guid? effectiveProjectId =
            selectedProjectId
            ?? defaultProjectId
            ?? projects.FirstOrDefault()?.Id;

        // Título do default (para exibição)
        string? defaultProjectTitle = null;
        if (defaultProjectId.HasValue)
        {
            defaultProjectTitle = await _db.Projects
                .AsNoTracking()
                .Where(p => p.Id == defaultProjectId.Value)
                .Select(p => p.Title)
                .SingleOrDefaultAsync(ct);
        }

        // Sessão ativa (não filtra por projeto)
        var active = await _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc == null &&
                        s.CanceledAtUtc == null)
            .Select(s => new ActiveSessionDto(
                s.Id,
                s.ProjectId,
                s.GoalId,
                s.StartTimeUtc))
            .SingleOrDefaultAsync(ct);

        // Base: sessões concluídas
        IQueryable<Session> completed = _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc != null &&
                        s.CanceledAtUtc == null);

        // Filtro por projeto efetivo (modo B)
        if (effectiveProjectId.HasValue)
        {
            completed = completed.Where(s => s.ProjectId == effectiveProjectId.Value);
        }

        // Totais
        var todayTotal = await completed
            .Where(s => s.StartTimeUtc >= todayStart)
            .SumAsync(s => EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value), ct);

        var weekTotal = await completed
            .Where(s => s.StartTimeUtc >= weekStart)
            .SumAsync(s => EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value), ct);

        // Últimas sessões (já filtradas pelo projeto efetivo, se existir)
        var lastSessions = await completed
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

        // =========================
        // DISTRIBUIÇÃO DA SEMANA (global por projeto do usuário)
        // =========================
        var weekByProject = await _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc != null &&
                        s.CanceledAtUtc == null &&
                        s.StartTimeUtc >= weekStart)
            .GroupBy(s => s.ProjectId)
            .Select(g => new
            {
                ProjectId = g.Key,
                Seconds = g.Sum(x => EF.Functions.DateDiffSecond(x.StartTimeUtc, x.EndTimeUtc!.Value))
            })
            .OrderByDescending(x => x.Seconds)
            .ToListAsync(ct);

        var titles = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .Select(p => new { p.Id, p.Title })
            .ToDictionaryAsync(x => x.Id, x => x.Title, ct);

        var projectTotals = weekByProject
            .Select(x => new ProjectTotalDto(
                x.ProjectId,
                titles.GetValueOrDefault(x.ProjectId, "Sem título"),
                x.Seconds
            ))
            .ToList();

// =========================
// ÚLTIMOS 7 DIAS (projeto efetivo)
// =========================
var last7Start = todayStart.AddDays(-6);

// Agrupa por componentes de data (traduzível pelo EF)
var dayGroups = await completed
    .Where(s => s.StartTimeUtc >= last7Start)
    .GroupBy(s => new
    {
        s.StartTimeUtc.Year,
        s.StartTimeUtc.Month,
        s.StartTimeUtc.Day
    })
    .Select(g => new
    {
        Day = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day), // DateTime UTC
        Seconds = g.Sum(x => EF.Functions.DateDiffSecond(x.StartTimeUtc, x.EndTimeUtc!.Value))
    })
    .ToListAsync(ct);

// Dicionário para lookup rápido
var dayDict = dayGroups
    .ToDictionary(x => x.Day.Date, x => x.Seconds);

// Monta os 7 dias garantidos (mesmo que 0)
var last7Days = Enumerable.Range(0, 7)
    .Select(i => last7Start.AddDays(i).UtcDateTime.Date)
    .Select(d =>
    {
        var seconds = dayDict.TryGetValue(d, out var v) ? v : 0;
        return new DayTotalDto(DateOnly.FromDateTime(d), seconds);
    })
    .ToList();

        return new DashboardStateDto(
            defaultProjectId,
            defaultProjectTitle,
            projects,
            active,
            0,      // DefaultProjectTotalSeconds (se quiser, calculamos depois)
            todayTotal,
            weekTotal,
            null,   // ActiveGoalTargetSeconds
            lastSessions,
            projectTotals,
            last7Days,
            "v2-last7days"
        );
    }
}