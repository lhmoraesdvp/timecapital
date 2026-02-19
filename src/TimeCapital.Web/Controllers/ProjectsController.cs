using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;
using TimeCapital.Domain.Entities;
using TimeCapital.Web.Infrastructure;

namespace TimeCapital.Web.Controllers;

public sealed record SetDefaultProjectRequest(Guid ProjectId);

[ApiController]
[Route("projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IUserContext _user;

    public ProjectsController(ApplicationDbContext db, IUserContext user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult> Get(CancellationToken ct)
    {
        var userId = _user.GetUserId();

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId && p.Status == ProjectStatus.Active)
            .OrderBy(p => p.Title)
            .Select(p => new { p.Id, p.Title })
            .ToListAsync(ct);

        return Ok(projects);
    }

    [HttpPost("set-default")]
    public async Task<ActionResult> SetDefault([FromBody] SetDefaultProjectRequest req, CancellationToken ct)
    {
        var userId = _user.GetUserId();

        var owns = await _db.Projects
            .AsNoTracking()
            .AnyAsync(p => p.Id == req.ProjectId && p.OwnerId == userId, ct);

        if (!owns)
            return NotFound(new { message = "Projeto não encontrado (ou não pertence ao usuário)." });

        var user = await _db.Users.SingleAsync(u => u.Id == userId, ct);
        user.DefaultProjectId = req.ProjectId;

        await _db.SaveChangesAsync(ct);

        return Ok(new { defaultProjectId = user.DefaultProjectId });
    }
}
