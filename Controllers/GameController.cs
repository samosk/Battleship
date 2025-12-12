using Microsoft.AspNetCore.Mvc;
using Battleship.Models;
using System.Security.Cryptography.X509Certificates;
namespace Battleship.Controllers;

public class GameController : Controller
{
    private readonly BattleshipContext _context;
    private readonly ILogger<GameController> _logger;

    public GameController(BattleshipContext context, ILogger<GameController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Board(int id)
    {
        var game = _context.Games.Find(id);
        if (game == null) return NotFound();

        var myShots = _context.Shots
            .Where(s => s.GameId == id && s.ShooterUserId == 1) // TODO: Replace with actual user ID
            .ToList();

        var opponentShots = _context.Shots
            .Where(s => s.GameId == id && s.ShooterUserId == 2) // TODO: Replace with actual user ID
            .ToList();

        var myShipViewModels = _context.Ships
            .Where(s => s.GameId == id && s.UserId == 1) // TODO: Replace with actual user ID
            .Select(s => new ShipViewModel()
            {
                X = s.X,
                Y = s.Y,
                Orientation = s.Orientation,
                Type = s.Type,
                IsMine = true,
                IsSunk = IsShipSunk(s, opponentShots)
            })
            .ToList();

        var opponentSunkShips = _context.Ships
            .Where(s => s.GameId == id && s.UserId == 2) // TODO: Replace with actual user ID
            .Select(s => new ShipViewModel()
            {
                X = s.X,
                Y = s.Y,
                Orientation = s.Orientation,
                Type = s.Type,
                IsMine = false,
                IsSunk = true
            })
            .ToList();


        var viewModel = new BoardViewModel
        {
            Game = game,
            MyShips = myShipViewModels,
            OpponentSunkShips = opponentSunkShips,// TODO: Populate opponent's sunk ships
            MyShots = myShots,
            OpponentShots = opponentShots
        };

        return View(viewModel);
    }

    private static int GetShipLength(Ship ship)
    {
        return ship.Type switch
        {
            ShipType.CARRIER => 5,
            ShipType.BATTLESHIP => 4,
            ShipType.DESTROYER => 3,
            ShipType.SUBMARINE => 3,
            ShipType.PATROL_BOAT => 2,
            _ => 0
        };
    }

    private static List<Position> GetPositionsForShip(Ship ship)
    {
        var positions = new List<Position>();

        for (int i = 0; i < GetShipLength(ship); i++)
        {
            var position = new Position();
            if (ship.Orientation == ShipOrientation.HORIZONTAL)
            {
                position.X = ship.X + i;
                position.Y = ship.Y;
            }
            else
            {
                position.X = ship.X;
                position.Y = ship.Y + i;
            }
            positions.Add(position);
        }

        return positions;
    }

    private static bool IsPositionInShip(Position position, Ship ship)
    {
        var length = GetShipLength(ship);

        var xStart = ship.X;
        var xEnd = ship.Orientation == ShipOrientation.HORIZONTAL ? xStart + length : xStart + 1;
        var yStart = ship.Y;
        var yEnd = ship.Orientation == ShipOrientation.VERTICAL ? yStart + length : yStart + 1;

        return (position.X >= xStart) && (position.X <= xEnd) && (position.Y >= yStart) && (position.Y <= yEnd);
    }

    private static bool IsShipSunk(Ship ship, List<Shot> shots)
    {
        return GetPositionsForShip(ship).All(p =>
            shots.Any(s => s.X == p.X && s.Y == p.Y)
        );
    }
}
