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
            .Where(p => p.Id == projectId && p.OwnerId == userId)
            .SingleOrDefaultAsync(ct);

        if (project == null)
            throw new InvalidOperationException("Projeto inválido.");

        var hasActive = await _db.Sessions
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

        active.EndTimeUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var durationSeconds = EF.Functions.DateDiffSecond(
            active.StartTimeUtc,
            active.EndTimeUtc!.Value);

        return new StopSessionResponse(
            active.Id,
            active.StartTimeUtc,
            active.EndTimeUtc!.Value,
            durationSeconds);
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
        var todayStart = new DateTimeOffset(now.Date, TimeSpan.Zero);

        var diff = ((int)now.DayOfWeek + 6) % 7;
        var weekStart = new DateTimeOffset(now.Date.AddDays(-diff), TimeSpan.Zero);

        var defaultProjectId = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.DefaultProjectId)
            .SingleAsync(ct);

        string? defaultProjectTitle = null;
        if (defaultProjectId.HasValue)
        {
            defaultProjectTitle = await _db.Projects
                .Where(p => p.Id == defaultProjectId.Value)
                .Select(p => p.Title)
                .SingleOrDefaultAsync(ct);
        }

        var projects = await _db.Projects
            .Where(p => p.OwnerId == userId)
            .OrderBy(p => p.Title)
            .Select(p => new ProjectListItemDto(p.Id, p.Title))
            .ToListAsync(ct);

        var active = await _db.Sessions
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc == null &&
                        s.CanceledAtUtc == null)
            .Select(s => new ActiveSessionDto(
                s.Id,
                s.ProjectId,
                s.GoalId,
                s.StartTimeUtc))
            .SingleOrDefaultAsync(ct);

        // Base: sessões concluídas (filtro do gráfico B entra aqui)
        var completed = _db.Sessions
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc != null &&
                        s.CanceledAtUtc == null);

        if (selectedProjectId.HasValue)
        {
            completed = completed.Where(s => s.ProjectId == selectedProjectId.Value);
        }

        // Totais (do projeto selecionado, modo B)
        var todayTotal = await completed
            .Where(s => s.StartTimeUtc >= todayStart)
            .SumAsync(s => EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value), ct);

        var weekTotal = await completed
            .Where(s => s.StartTimeUtc >= weekStart)
            .SumAsync(s => EF.Functions.DateDiffSecond(s.StartTimeUtc, s.EndTimeUtc!.Value), ct);

        var lastSessions = await completed
            .OrderByDescending(s => s.EndTimeUtc)
            .Take(10)
            .Join(_db.Projects,
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
        // ÚLTIMOS 7 DIAS (do projeto selecionado, modo B)
        // =========================
        var last7Start = todayStart.AddDays(-6);

        var dayGroups = await completed
            .Where(s => s.StartTimeUtc >= last7Start)
            .GroupBy(s => s.StartTimeUtc.Date)
            .Select(g => new
            {
                Day = g.Key, // DateTime
                Seconds = g.Sum(x => EF.Functions.DateDiffSecond(x.StartTimeUtc, x.EndTimeUtc!.Value))
            })
            .ToListAsync(ct);

        var dayDict = dayGroups.ToDictionary(x => x.Day, x => x.Seconds);

        var last7Days = Enumerable.Range(0, 7)
            .Select(i => last7Start.AddDays(i).UtcDateTime.Date) // DateTime (UTC date)
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
            0,              // DefaultProjectTotalSeconds (se quiser eu calculo depois)
            todayTotal,
            weekTotal,
            null,           // ActiveGoalTargetSeconds
            lastSessions,
            projectTotals,
            last7Days,
            "v2-last7days"
        );
    }
}
