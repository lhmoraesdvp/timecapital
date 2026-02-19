using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;

namespace TimeCapital.Web.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProjectsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("set-default")]
    public async Task<IActionResult> SetDefault(
        [FromBody] SetDefaultProjectRequest request,
        CancellationToken ct)
    {
        var userId = "luis";

        var user = await _db.Users
            .SingleAsync(u => u.Id == userId, ct);

        var projectExists = await _db.Projects
            .AnyAsync(p => p.Id == request.ProjectId && p.OwnerId == userId, ct);

        if (!projectExists)
            return BadRequest("Projeto inválido.");

        user.DefaultProjectId = request.ProjectId;

        await _db.SaveChangesAsync(ct);

        return Ok();
    }
}

public record SetDefaultProjectRequest(Guid ProjectId);
