using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Battleship.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Battleship.Hubs;

namespace Battleship.Controllers;

public class HomeController : Controller
{
    private readonly BattleshipDbContext _dbContext;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<GameController> _logger;

    public HomeController(BattleshipDbContext dbContext, IHubContext<GameHub> hubContext, UserManager<User> userManager, ILogger<GameController> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _userManager = userManager;
        _logger = logger;
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
