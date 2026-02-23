// TimeCapital.Application/Sessions/ISessionService.cs
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

Task DeleteSessionAsync(string userId, Guid sessionId, CancellationToken ct);
    // ATUALIZADO: aceita projectId opcional (modo B)
    Task<DashboardStateDto> GetDashboardStateAsync(
        string userId,
        Guid? selectedProjectId = null,
        CancellationToken ct = default);
}
