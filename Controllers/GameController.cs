using Microsoft.AspNetCore.Mvc;
using Battleship.Models;
namespace Battleship.Controllers;

public class GameController : Controller
{
    private readonly ILogger<GameController> _logger;

    public GameController(ILogger<GameController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Board()
    {
        return View();
    }
}
