using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Battleship.Models;
using NuGet.Protocol;
using System.Text.Json;

namespace Battleship.Controllers;

public class GameController : Controller
{
    private readonly BattleshipContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<GameController> _logger;

    public GameController(BattleshipContext context, UserManager<User> userManager, ILogger<GameController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Setup(int id)
    {
        var game = _context.Games.Find(id);
        if (game == null) return NotFound();

        var shipTypes = new List<ShipType> {
            ShipType.CARRIER,
            ShipType.BATTLESHIP,
            ShipType.DESTROYER,
            ShipType.SUBMARINE,
            ShipType.PATROL_BOAT
        };

        var viewModel = new SetupViewModel
        {
            Game = game,
            Ships = shipTypes.Select(shipType => new ShipViewModel()
            {
                X = 0,
                Y = 0,
                Orientation = ShipOrientation.HORIZONTAL,
                Type = shipType,
                IsMine = true,
                IsSunk = false
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Setup(int id, SetupViewModel vm)
    {
        // TODO: Set game state and so forth...
        return Ok();
    }

    [HttpGet]
    public IActionResult Board(int id)
    {
        var game = _context.Games.Find(id);
        if (game == null) return NotFound();
        
        var userId = _userManager.GetUserId(HttpContext.User);

        var myShots = _context.Shots
            .Where(s => s.GameId == id && s.ShooterUserId == userId)
            .ToList();

        var opponentShots = _context.Shots
            .Where(s => s.GameId == id && s.ShooterUserId != userId)
            .ToList();

        var myShips = _context.Ships
            .Where(s => s.GameId == id && s.UserId == userId)
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

        var opponentShips = _context.Ships
            .Where(s => s.GameId == id && s.UserId != userId)
            .Select(s => new ShipViewModel()
            {
                X = s.X,
                Y = s.Y,
                Orientation = s.Orientation,
                Type = s.Type,
                IsMine = false,
                IsSunk = IsShipSunk(s, myShots)
            })
            .ToList();

        var opponentSunkShips = opponentShips
            .Where(s => s.IsSunk)
            .ToList();

        var viewModel = new BoardViewModel
        {
            Game = game,
            MyShips = myShips,
            OpponentSunkShips = opponentSunkShips,
            MyShots = myShots,
            OpponentShots = opponentShots
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Shoot(int id, BoardViewModel vm)
    {
        // Due to difficulties with using multiple view models in the same view, we have gone with a janky solution.
        // Everything inside BoardViewModel is null except for SelectedShotPosition.

        var game = _context.Games.Find(id);
        if (game == null) return NotFound();
        if (vm.SelectedShotPosition == null) return BadRequest();

        if (game.State != GameState.PLAY)
        {
            return BadRequest("Shooting is not allowed right now.");
        }

        var userId = _userManager.GetUserId(HttpContext.User);

        if (game.ActiveUserId != userId)
        {
            return BadRequest("It's not your turn.");
        }

        var components = vm.SelectedShotPosition.Split(",");
        int x;
        int y;

        try
        {
            x = int.Parse(components[0]);
            y = int.Parse(components[1]);
        }
        catch (Exception)
        {
            return BadRequest();
        }

        var myShots = _context.Shots
            .Where(s => s.GameId == id && s.ShooterUserId == userId)
            .ToList();

        if (myShots.Any(s => s.X == x && s.Y == y))
        {
            return BadRequest("You have already shot at this position.");
        }

        var shot = new Shot
        {
            GameId = id,
            ShooterUserId = userId,
            X = x,
            Y = y,
            Outcome = ShotOutcome.MISS // Default, may change below
        };

        var position = new Position { X = x, Y = y };

        var myShotsNew = myShots.Append(shot).ToList();

        var opponentShips = _context.Ships
            .Where(s => s.GameId == id && s.UserId != userId)
            .ToList();

        opponentShips.ForEach(ship =>
        {
            if (IsPositionInShip(position, ship))
            {
                shot.Outcome = ShotOutcome.HIT;
                if (IsShipSunk(ship, myShotsNew))
                {
                    shot.Outcome = ShotOutcome.SINK;
                }
            }
        });

        var sinkCount = myShotsNew
            .FindAll(shot => shot.Outcome == ShotOutcome.SINK)
            .Count();
        if (sinkCount == opponentShips.Count())
        {
            game.State = GameState.END;
        }
        else if (shot.Outcome == ShotOutcome.MISS)
        {
            if (game.ActiveUserId == game.User1Id)
            {
                game.ActiveUserId = game.User2Id;
            }
            else
            {
                game.ActiveUserId = game.User1Id;
                game.TurnCount++;
            }
        }

        _context.Shots.Add(shot);
        _context.SaveChanges();
        return RedirectToAction("Board", new { id });
    }

    public static int GetShipLength(Ship ship)
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

    public static int GetShipLength(ShipViewModel vm)
    {
        return GetShipLength(new Ship(vm));
    }

    public static List<Position> GetPositionsForShip(Ship ship)
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

    public static bool IsPositionInShip(Position position, Ship ship)
    {
        var length = GetShipLength(ship);

        var xStart = ship.X;
        var xEnd = ship.Orientation == ShipOrientation.HORIZONTAL ? xStart + length - 1 : xStart;
        var yStart = ship.Y;
        var yEnd = ship.Orientation == ShipOrientation.VERTICAL ? yStart + length - 1 : yStart;

        return (position.X >= xStart) && (position.X <= xEnd) && (position.Y >= yStart) && (position.Y <= yEnd);
    }

    public static bool IsPositionInShip(Position position, ShipViewModel vm)
    {
        return IsPositionInShip(position, new Ship(vm));
    }

    public static bool IsShipSunk(Ship ship, List<Shot> candidateShots)
    {
        return GetPositionsForShip(ship).All(p =>
            candidateShots.Any(s => s.X == p.X && s.Y == p.Y)
        );
    }
}
