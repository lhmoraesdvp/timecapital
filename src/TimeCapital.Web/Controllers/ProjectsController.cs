using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;
using TimeCapital.Domain.Entities;
using TimeCapital.Web.Infrastructure;

namespace TimeCapital.Web.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
 private readonly IUserContext _user;
    public ProjectsController(ApplicationDbContext db,IUserContext user)
    {
        _db = db;
           _user = user;
    }

    [HttpPost("set-default")]
    public async Task<IActionResult> SetDefault(
        [FromBody] SetDefaultProjectRequest request,
        CancellationToken ct)
    {
        var userId = _user.GetUserId();

        if (request.ProjectId == Guid.Empty)
            return BadRequest("Projeto inválido.");

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
        var userId = _user.GetUserId();

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

        // Se não tem default ainda, seta automaticamente
        var user = await _db.Users.SingleAsync(u => u.Id == userId, ct);
        if (user.DefaultProjectId == null)
            user.DefaultProjectId = project.Id;

        await _db.SaveChangesAsync(ct);

        // retorno que o front espera (id / title)
        return Ok(new CreateProjectResponse(project.Id, project.Title));
    }

    // ✅ NOVO: excluir projeto (sem cascade)
    // Regra: se tiver sessões vinculadas -> 400
    // Também limpa DefaultProjectId se for o projeto excluído.
    [HttpPost("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] DeleteProjectRequest request,
        CancellationToken ct)
    {
        var userId = _user.GetUserId();

        if (request.ProjectId == Guid.Empty)
            return BadRequest("Projeto inválido.");

        // carrega projeto do usuário
        var project = await _db.Projects
            .SingleOrDefaultAsync(p => p.Id == request.ProjectId && p.OwnerId == userId, ct);

        if (project is null)
            return BadRequest("Projeto inválido.");

        // bloqueia se existir sessão ligada ao projeto
        // (ajuste o DbSet/nome se sua entidade não for "Sessions" ou não tiver ProjectId)
var sessions = await _db.Sessions
    .Where(s => s.ProjectId == request.ProjectId)
    .ToListAsync(ct);

_db.Sessions.RemoveRange(sessions);
        // limpa default se estiver apontando para este projeto
        var user = await _db.Users.SingleAsync(u => u.Id == userId, ct);
if (user.DefaultProjectId == request.ProjectId)
{
    var nextProject = await _db.Projects
        .Where(p => p.OwnerId == userId && p.Id != request.ProjectId)
        .OrderBy(p => p.Title)
        .Select(p => (Guid?)p.Id)
        .FirstOrDefaultAsync(ct);

    user.DefaultProjectId = nextProject; // pode ser null se não houver mais projetos
}

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);

        return Ok();
    }
}

public record SetDefaultProjectRequest(Guid ProjectId);
public record CreateProjectRequest(string Title);
public record CreateProjectResponse(Guid Id, string Title);
public record DeleteProjectRequest(Guid ProjectId);