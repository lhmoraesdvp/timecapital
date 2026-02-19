using Microsoft.AspNetCore.Mvc;
using TimeCapital.Application.Common;
using TimeCapital.Application.Sessions;
using TimeCapital.Web.Infrastructure;

namespace TimeCapital.Web.Controllers;

[ApiController]
[Route("sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly ISessionService _sessions;
    private readonly IUserContext _user;

    public SessionsController(ISessionService sessions, IUserContext user)
    {
        _sessions = sessions;
        _user = user;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartSessionResponse>> Start([FromBody] StartSessionRequest req, CancellationToken ct)
    {
        try
        {
            var userId = _user.GetUserId();
            var result = await _sessions.StartSessionAsync(userId, req, ct);
            return Ok(result);
        }
        catch (ValidationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (ForbiddenException ex) { return StatusCode(403, new { message = ex.Message }); }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ConflictException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("stop")]
    public async Task<ActionResult<StopSessionResponse>> Stop(CancellationToken ct)
    {
        try
        {
            var userId = _user.GetUserId();
            var result = await _sessions.StopSessionAsync(userId, ct);
            return Ok(result);
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ValidationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("cancel")]
    public async Task<ActionResult<CancelSessionResponse>> Cancel(CancellationToken ct)
    {
        try
        {
            var userId = _user.GetUserId();
            var result = await _sessions.CancelActiveSessionAsync(userId, ct);
            return Ok(result);
        }
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
