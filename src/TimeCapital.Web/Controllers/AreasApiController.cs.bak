using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeCapital.Data;
using TimeCapital.Web.Security;

namespace TimeCapital.Web.Controllers;

[ApiController]
[Route("api/areas")]
public class AreasApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AreasApiController(ApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = _currentUser.UserId;

        var areas = await _db.Areas
            .Where(a => a.UserId == userId && !a.IsArchived)
            .OrderBy(a => a.Name)
            .Select(a => new { id = a.Id, name = a.Name, color = a.Color })
            .ToListAsync();

        return Ok(areas);
    }
}
