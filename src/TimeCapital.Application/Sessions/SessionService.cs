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
        // projeto pertence ao usuário?
        var project = await _db.Projects
            .Where(p => p.Id == projectId && p.OwnerId == userId)
            .SingleOrDefaultAsync(ct);

        if (project == null)
            throw new InvalidOperationException("Projeto inválido.");

        // se goal informado, apenas validar que pertence ao projeto
        if (goalId.HasValue)
        {
            var goalOk = await _db.Goals
                .AnyAsync(g => g.Id == goalId.Value && g.ProjectId == projectId, ct);

            if (!goalOk)
                throw new InvalidOperationException("Goal inválido.");
        }

        // já existe sessão ativa?
        var hasActive = await _db.Sessions
            .AnyAsync(s => s.UserId == userId &&
                           s.EndTimeUtc == null &&
                           s.CanceledAtUtc == null, ct);

        if (hasActive)
            throw new InvalidOperationException("Já existe sessão ativa.");

        var now = DateTimeOffset.UtcNow;

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProjectId = projectId,
            GoalId = goalId,
            StartTimeUtc = now
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

        var durationSeconds = await _db.Sessions
            .Where(s => s.Id == active.Id)
            .Select(s =>
                EF.Functions.DateDiffSecond(
                    s.StartTimeUtc,
                    s.EndTimeUtc!.Value))
            .SingleAsync(ct);

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
    // DASHBOARD STATE
    // =========================
    public async Task<DashboardStateDto> GetDashboardStateAsync(
        string userId,
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

        var completed = _db.Sessions
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc != null &&
                        s.CanceledAtUtc == null);

        var todayTotal = await completed
            .Where(s => s.StartTimeUtc >= todayStart)
            .SumAsync(s =>
                EF.Functions.DateDiffSecond(
                    s.StartTimeUtc,
                    s.EndTimeUtc!.Value), ct);

        var weekTotal = await completed
            .Where(s => s.StartTimeUtc >= weekStart)
            .SumAsync(s =>
                EF.Functions.DateDiffSecond(
                    s.StartTimeUtc,
                    s.EndTimeUtc!.Value), ct);

        var defaultProjectTotal = 0;
        if (defaultProjectId.HasValue)
        {
            defaultProjectTotal = await completed
                .Where(s => s.ProjectId == defaultProjectId.Value)
                .SumAsync(s =>
                    EF.Functions.DateDiffSecond(
                        s.StartTimeUtc,
                        s.EndTimeUtc!.Value), ct);
        }

        // últimas sessões (fallback se não houver default)
        IQueryable<Session> lastQuery = completed;

        if (defaultProjectId.HasValue)
            lastQuery = lastQuery.Where(s => s.ProjectId == defaultProjectId.Value);

        var lastSessions = await lastQuery
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
                    EF.Functions.DateDiffSecond(
                        s.StartTimeUtc,
                        s.EndTimeUtc!.Value)
                ))
            .ToListAsync(ct);
string? defaultProjectTitle = null;

if (defaultProjectId.HasValue)
{
    defaultProjectTitle = await _db.Projects
        .Where(p => p.Id == defaultProjectId.Value)
        .Select(p => p.Title)
        .SingleOrDefaultAsync(ct);
}

    return new DashboardStateDto(
    defaultProjectId,
    defaultProjectTitle,
    projects,
    active,
    defaultProjectTotal,
    todayTotal,
    weekTotal,
    null,
    lastSessions,
    new List<ProjectTotalDto>() // <-- ESTE É O QUE ESTÁ FALTANDO
);

    }
}
