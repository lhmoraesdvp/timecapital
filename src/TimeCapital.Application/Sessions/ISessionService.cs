namespace TimeCapital.Application.Sessions;

public interface ISessionService
{
    Task<StartSessionResponse> StartSessionAsync(
        string userId,
        Guid projectId,
        Guid? goalId,
        CancellationToken ct = default);

    Task<StopSessionResponse> StopSessionAsync(
        string userId,
        CancellationToken ct = default);

    Task CancelActiveSessionAsync(
        string userId,
        CancellationToken ct = default);

    Task<DashboardStateDto> GetDashboardStateAsync(
        string userId,
        CancellationToken ct = default);
}
