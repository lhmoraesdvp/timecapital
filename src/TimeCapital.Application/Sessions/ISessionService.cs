using System.Threading;
using System.Threading.Tasks;

namespace TimeCapital.Application.Sessions;

public interface ISessionService
{
    Task<StartSessionResponse> StartSessionAsync(string userId, StartSessionRequest req, CancellationToken ct = default);
    Task<StopSessionResponse> StopSessionAsync(string userId, CancellationToken ct = default);
    Task<CancelSessionResponse> CancelActiveSessionAsync(string userId, CancellationToken ct = default);
    Task<DashboardStateDto> GetDashboardStateAsync(string userId, CancellationToken ct = default);
}
