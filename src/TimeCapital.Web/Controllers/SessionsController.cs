using Microsoft.AspNetCore.Mvc;
using TimeCapital.Application.Sessions;

namespace TimeCapital.Web.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    // =========================
    // START
    // =========================
    [HttpPost("start")]
    public async Task<IActionResult> Start(
        [FromBody] StartSessionRequest request,
        CancellationToken ct)
    {
        var userId = "luis"; // ambiente local sem identity

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
        var userId = "luis";

        var result = await _sessionService.StopSessionAsync(userId, ct);

        return Ok(result);
    }

    // =========================
    // CANCEL
    // =========================
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken ct)
    {
        var userId = "luis";

        await _sessionService.CancelActiveSessionAsync(userId, ct);

        return Ok();
    }

    // =========================
    // DELETE
    // =========================
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteSessionRequest request,
        CancellationToken ct)
    {
        var userId = "luis";

        if (request is null || request.SessionId == Guid.Empty)
            return BadRequest(new { message = "SessionId inválido." });

        try
        {
            await _sessionService.DeleteSessionAsync(userId, request.SessionId, ct);
            return Ok(new { ok = true });
        }
        catch (InvalidOperationException ex)
        {
            // evita 500 e devolve mensagem pro seu showToast()
            return BadRequest(new { message = ex.Message });
        }
    }
}

public sealed class DeleteSessionRequest
{
    public Guid SessionId { get; set; }
}