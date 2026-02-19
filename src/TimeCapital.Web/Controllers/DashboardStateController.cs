using Microsoft.AspNetCore.Mvc;
using TimeCapital.Application.Sessions;
using TimeCapital.Web.Infrastructure;

namespace TimeCapital.Web.Controllers;

[ApiController]
[Route("")]
public sealed class DashboardStateController : ControllerBase
{
    private readonly ISessionService _sessions;
    private readonly IUserContext _user;

    public DashboardStateController(ISessionService sessions, IUserContext user)
    {
        _sessions = sessions;
        _user = user;
    }

    [HttpGet("dashboard-state")]
    public async Task<ActionResult<DashboardStateDto>> Get(CancellationToken ct)
    {
        var userId = _user.GetUserId();
        var dto = await _sessions.GetDashboardStateAsync(userId, ct);
        return Ok(dto);
    }
}
