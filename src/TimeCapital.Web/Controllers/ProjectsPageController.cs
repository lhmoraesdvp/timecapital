using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;
using TimeCapital.Domain.Entities;
using TimeCapital.Web.Infrastructure;

namespace TimeCapital.Web.Controllers;

public sealed class ProjectsPageController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IUserContext _user;

    public ProjectsPageController(ApplicationDbContext db, IUserContext user)
    {
        _db = db;
        _user = user;
    }

    // GET /projects
    [HttpGet("/projects")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = _user.GetUserId();

        var projects = await _db.Projects
            .Where(p => p.OwnerId == userId)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);

        var vm = new ProjectsIndexVm
        {
            Projects = projects
        };

        return View(vm);
    }

    // POST /projects
    [HttpPost("/projects")]
    public async Task<IActionResult> Create(string title, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title))
            return RedirectToAction(nameof(Index));

        var userId = _user.GetUserId();

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            OwnerId = userId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        return RedirectToAction(nameof(Index));
    }
}

public sealed class ProjectsIndexVm
{
    public IReadOnlyList<Project> Projects { get; set; } = new List<Project>();
}
