using System.Diagnostics;
using Microsoft.AspNetCore.Authorization; // ✅ adicionar
using Microsoft.AspNetCore.Mvc;
using TimeCapital.Web.Models;

namespace TimeCapital.Web.Controllers;

[Authorize] // ✅ protege todo o controller
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous] // opcional, mas recomendado
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}