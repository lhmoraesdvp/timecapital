using Microsoft.AspNetCore.Mvc;
using TimeCapital.Web.Security;

namespace TimeCapital.Web.Controllers;

public class MeController : Controller
{
    private readonly ICurrentUser _currentUser;

    public MeController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet("/me")]
    public IActionResult Index()
        => Content($"UserId={_currentUser.UserId} | Name={_currentUser.DisplayName}");
}
