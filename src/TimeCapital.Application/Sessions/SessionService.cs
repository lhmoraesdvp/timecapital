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

        var completed = _db.Sessions
            .Where(s => s.UserId == userId &&
                        s.EndTimeUtc != null &&
                        s.CanceledAtUtc == null);

        // Totais globais
        var globalToday = await completed
            .Where(s => s.StartTimeUtc >= todayStart)
            .SumAsync(s =>
                EF.Functions.DateDiffSecond(
                    s.StartTimeUtc,
                    s.EndTimeUtc!.Value), ct);

        var globalWeek = await completed
            .Where(s => s.StartTimeUtc >= weekStart)
            .SumAsync(s =>
                EF.Functions.DateDiffSecond(
                    s.StartTimeUtc,
                    s.EndTimeUtc!.Value), ct);

        int defaultProjectTotal = 0;
        int projectTodayTotal = 0;
        int projectWeekTotal = 0;

        if (defaultProjectId.HasValue)
        {
            defaultProjectTotal = await completed
                .Where(s => s.ProjectId == defaultProjectId.Value)
                .SumAsync(s =>
                    EF.Functions.DateDiffSecond(
                        s.StartTimeUtc,
                        s.EndTimeUtc!.Value), ct);

            projectTodayTotal = await completed
                .Where(s => s.ProjectId == defaultProjectId.Value &&
                            s.StartTimeUtc >= todayStart)
                .SumAsync(s =>
                    EF.Functions.DateDiffSecond(
                        s.StartTimeUtc,
                        s.EndTimeUtc!.Value), ct);

            projectWeekTotal = await completed
                .Where(s => s.ProjectId == defaultProjectId.Value &&
                            s.StartTimeUtc >= weekStart)
                .SumAsync(s =>
                    EF.Functions.DateDiffSecond(
                        s.StartTimeUtc,
                        s.EndTimeUtc!.Value), ct);
        }

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
                    EF.Functions.DateDiffSecond(
                        s.StartTimeUtc,
                        s.EndTimeUtc!.Value)
                ))
            .ToListAsync(ct);

        return new DashboardStateDto(
            defaultProjectId,
            defaultProjectTitle,
            projects,
            active,
            defaultProjectTotal,
            projectTodayTotal,
            projectWeekTotal,
            null,
            lastSessions,
            new List<ProjectTotalDto>()
        );
    }
}
