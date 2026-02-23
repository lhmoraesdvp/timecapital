using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeCapital.Application.Sessions;
using TimeCapital.Web.Infrastructure;

namespace TimeCapital.Web.Controllers;

[ApiController]
[Authorize]
[Route("sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IUserContext _user;

    public SessionsController(ISessionService sessionService, IUserContext user)
    {
        _sessionService = sessionService;
        _user = user;
    }

    // =========================
    // START
    // =========================
    [HttpPost("start")]
    public async Task<IActionResult> Start(
        [FromBody] StartSessionRequest request,
        CancellationToken ct)
    {
        var userId = _user.GetUserId();

        var result = await _sessionService.StartSessionAsync(
            userId,
            request.ProjectId,
            request.GoalId,
            ct);

        return Ok(result);
    }

    // =========================
    // STOP
    // =========================
    [HttpPost("stop")]
    public async Task<IActionResult> Stop(CancellationToken ct)
    {
        var userId = _user.GetUserId();
        var result = await _sessionService.StopSessionAsync(userId, ct);
        return Ok(result);
    }

    // =========================
    // CANCEL
    // =========================
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken ct)
    {
        var userId = _user.GetUserId();
        await _sessionService.CancelActiveSessionAsync(userId, ct);
        return Ok();
    }

    // =========================
    // DELETE SESSION  ✅ voltou
    // =========================
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteSessionRequest request,
        CancellationToken ct)
    {
        var userId = _user.GetUserId();

        await _sessionService.DeleteSessionAsync(
            userId,
            request.SessionId,
            ct);

        return Ok();
    }
}

// DTO do request (pode ficar aqui ou em arquivo separado)
public sealed class DeleteSessionRequest
{
    public Guid SessionId { get; set; }
}