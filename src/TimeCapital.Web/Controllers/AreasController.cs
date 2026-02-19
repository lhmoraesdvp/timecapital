using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;
using TimeCapital.Domain.Entities;
using TimeCapital.Web.Security;
using TimeCapital.Web.ViewModels;

namespace TimeCapital.Web.Controllers;

public class AreasController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AreasController(ApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("/areas")]
    public async Task<IActionResult> Index()
    {
        var userId = _currentUser.UserId;

        var areas = await _db.Areas
            .Where(a => a.UserId == userId && !a.IsArchived)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return View(areas);
    }
}
