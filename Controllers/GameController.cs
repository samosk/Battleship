using Microsoft.AspNetCore.Mvc;
using Battleship.Models;
namespace Battleship.Controllers;

public class GameController : Controller
{
    private readonly IConfiguration _config;
    private readonly ILogger<GameController> _logger;

    public GameController(IConfiguration config, ILogger<GameController> logger)
    {
        _config = config;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Board(int id)
    {
        using (var context = new BattleshipContext(_config))
        {
            var game = context.Games.Find(id);
            if (game == null) return NotFound();

            var myShipViewModels = context.Ships
                .Where(s => s.GameId == id && s.UserId == 1) // TODO: Replace with actual user ID
                .Select(s => new ShipViewModel()
                {
                    X = s.X,
                    Y = s.Y,
                    Orientation = s.Orientation,
                    Type = s.Type,
                    IsMine = true,
                    IsSunk = false // TODO: Determine if the ship is sunk
                })
                .ToList();

            var myShots = context.Shots
                .Where(s => s.GameId == id && s.ShooterUserId == 1) // TODO: Replace with actual user ID
                .ToList();

            var opponentShots = context.Shots
                .Where(s => s.GameId == id && s.ShooterUserId == 2) // TODO: Replace with actual user ID
                .ToList();

            var viewModel = new BoardViewModel
            {
                Game = game,
                OpponentSunkShips = new List<ShipViewModel>(), // TODO: Populate opponent's sunk ships
                MyShips = myShipViewModels,
                MyShots = myShots,
                OpponentShots = opponentShots
            };

            return View(viewModel);
        }
    }
}
