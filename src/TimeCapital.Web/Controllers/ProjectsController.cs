using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;
using TimeCapital.Domain.Entities;

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

    // ✅ NOVO: criar projeto
    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var userId = "luis";

        var title = (request.Title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Título é obrigatório.");

        // (opcional) evita duplicado por usuário
        var exists = await _db.Projects
            .AnyAsync(p => p.OwnerId == userId && p.Title == title, ct);

        if (exists)
            return BadRequest("Já existe um projeto com esse nome.");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            Title = title,
        };

        _db.Projects.Add(project);

        // Se quiser: se não tem default ainda, setar automaticamente
        var user = await _db.Users.SingleAsync(u => u.Id == userId, ct);
        if (user.DefaultProjectId == null)
            user.DefaultProjectId = project.Id;

        await _db.SaveChangesAsync(ct);

        // retorno que o front espera
        return Ok(new CreateProjectResponse(project.Id, project.Title));
    }
}

public record SetDefaultProjectRequest(Guid ProjectId);

public record CreateProjectRequest(string Title);

public record CreateProjectResponse(Guid Id, string Title);