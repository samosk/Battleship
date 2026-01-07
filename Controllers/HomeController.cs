using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Battleship.Models;
using Microsoft.AspNetCore.Identity;

namespace Battleship.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<User> _userManager;

    public HomeController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var userId = _userManager.GetUserId(User);
        if(userId != null)
        {
            return RedirectToAction("List", "Game");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
