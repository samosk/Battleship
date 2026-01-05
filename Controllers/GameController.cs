using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Battleship.Models;
using Battleship.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Battleship.Controllers;

public class GameController : Controller
{
    private readonly BattleshipDbContext _dbContext;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<GameController> _logger;

    public GameController(BattleshipDbContext dbContext, IHubContext<GameHub> hubContext, UserManager<User> userManager, ILogger<GameController> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult List()
    {
        var userId = _userManager.GetUserId(User);
        var myGames = _dbContext.Games
            .Where(g => g.User1Id == userId || g.User2Id == userId)
            .Select(g => new GameViewModel
            {
                GameId = g.GameId,
                Name = g.Name,
                User1 = g.User1,
                User1Id = g.User1Id,
                User2 = g.User2,
                User2Id = g.User2Id,
                IsMyTurn = g.ActiveUserId == userId,
                CreatedAt = g.CreatedAt,
                ModifiedAt = g.ModifiedAt,
                State = g.State
            })
            .OrderByDescending(gv => gv.ModifiedAt).ToList();
        myGames.ForEach(async gv =>
        {
            gv.User1 = gv.User1Id == null ? null : await _userManager.FindByIdAsync(gv.User1Id);
            gv.User2 = gv.User2Id == null ? null : await _userManager.FindByIdAsync(gv.User2Id);
        });
        return View(myGames);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(CreateViewModel vm)
    {
        var myUserId = _userManager.GetUserId(User);
        if (myUserId == null)
        {
            return Unauthorized();
        }

        User? opponent = null;

        if (vm.OpponentUserName == null)
        {
            ModelState.AddModelError("OpponentUserName", "You need to pick an opponent.");
        }
        else
        {
            opponent = await _userManager.FindByNameAsync(vm.OpponentUserName);
            if (opponent == null)
            {
                ModelState.AddModelError("OpponentUserName", "No such user found.");
            }
            else if (opponent.Id == myUserId)
            {
                ModelState.AddModelError("OpponentUserName", "Pick someone other than yourself, you silly goose! 🪿");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var game = new Game
        {
            Name = vm.GameName,
            User1Id = myUserId,
            User2Id = opponent!.Id,
            ActiveUserId = myUserId,
            State = GameState.SETUP
        };

        _dbContext.Games.Add(game);
        _dbContext.SaveChanges();
        return RedirectToAction("Board", new { id = game.GameId });
    }

    [HttpGet]
    [Authorize]
    public IActionResult Setup(int id)
    {
        var game = _dbContext.Games.Find(id);
        if (game == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (userId != game.User1Id && userId != game.User2Id)
        {
            return Forbid();
        }

        var myShipCount = _dbContext.Ships
            .Where(s => s.GameId == id && s.UserId == userId)
            .Count();

        var opponentShipCount = _dbContext.Ships
            .Where(s => s.GameId == id && s.UserId != userId)
            .Count();

        if (game.State != GameState.SETUP || (myShipCount > 0 && opponentShipCount > 0))
        {
            return RedirectToAction("Board", new { id });
        }

        var isWaitingForOpponent = myShipCount > 0 && opponentShipCount == 0;

        var shipTypes = Enum.GetValues<ShipType>();

        var viewModel = new SetupViewModel
        {
            Game = game,
            IsWaitingForOpponent = isWaitingForOpponent,
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
    [Authorize]
    public async Task<IActionResult> Setup(int id, SetupViewModel vm)
    {
        var game = _dbContext.Games.Find(id);
        if (game == null) return NotFound();

        if (game.State != GameState.SETUP)
        {
            return BadRequest("This game has already started.");
        }

        var userId = _userManager.GetUserId(User);
        if (userId != game.User1Id && userId != game.User2Id)
        {
            return Forbid();
        }

        var myShips = vm.Ships
            .Select(shipVm => new Ship(shipVm))
            .ToList();

        var selectedShipTypes = myShips.Select(s => s.Type).ToList();
        var expectedShipTypes = Enum.GetValues<ShipType>().ToList();
        var areShipTypesCorrect =
            (selectedShipTypes.Count() == expectedShipTypes.Count())
            && !selectedShipTypes.Except(expectedShipTypes).Any();

        if (!areShipTypesCorrect || myShips.Any(IsShipOutOfBounds) || AreShipsOverlapping(myShips))
        {
            return BadRequest("Invalid ship placement.");
        }

        myShips.ForEach(ship =>
        {
            ship.GameId = id;
            ship.UserId = userId;
            _dbContext.Ships.Add(ship);
        });

        var opponentShips = _dbContext.Ships
            .Where(s => s.GameId == id && s.UserId != userId)
            .ToList();

        if (myShips.Count() > 0 && opponentShips.Count() > 0)
        {
            game.State = GameState.PLAY;
            game.ActiveUserId = game.User1Id;
            game.TurnCount = 1;

            _dbContext.SaveChanges();
            await _hubContext.Clients.All.SendAsync("GameUpdated", id.ToString());
            return RedirectToAction("Board", new { id });
        }

        _dbContext.SaveChanges();
        return RedirectToAction("Setup", new { id });
    }

    [HttpGet]
    [Authorize]
    public IActionResult Board(int id)
    {
        var game = _dbContext.Games.Find(id);
        if (game == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (userId != game.User1Id && userId != game.User2Id)
        {
            return Forbid();
        }

        if (game.State == GameState.SETUP)
        {
            return RedirectToAction("Setup", new { id });
        }

        var myShots = _dbContext.Shots
            .Where(s => s.GameId == id && s.ShooterUserId == userId)
            .ToList();

        var opponentShots = _dbContext.Shots
            .Where(s => s.GameId == id && s.ShooterUserId != userId)
            .ToList();

        var myShips = _dbContext.Ships
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

        var opponentShips = _dbContext.Ships
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
    [Authorize]
    public async Task<IActionResult> Shoot(int id, BoardViewModel vm)
    {
        // Due to difficulties with using multiple view models in the same view, we have gone with a janky solution.
        // Everything inside BoardViewModel is null except for SelectedShotPosition.

        var game = _dbContext.Games.Find(id);
        if (game == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (userId != game.User1Id && userId != game.User2Id)
        {
            return Forbid();
        }

        if (vm.SelectedShotPosition == null) return BadRequest();

        if (game.State != GameState.PLAY)
        {
            return BadRequest("Shooting is not allowed right now.");
        }

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

        var myShots = _dbContext.Shots
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

        var opponentShips = _dbContext.Ships
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
        _dbContext.Shots.Add(shot);

        game.ModifiedAt = DateTime.Now;
        _dbContext.SaveChanges();

        await _hubContext.Clients.All.SendAsync("GameUpdated", id.ToString());

        return RedirectToAction("Board", new { id });
    }

    public static bool IsShipOutOfBounds(Ship ship)
    {
        if (ship.X < 1 || ship.Y < 1) return true;

        if (ship.Orientation == ShipOrientation.HORIZONTAL)
        {
            return ship.X + GetShipLength(ship) - 1 > 10;
        }
        return ship.Y + GetShipLength(ship) - 1 > 10;
    }

    public static bool AreShipsOverlapping(List<Ship> boardShips)
    {
        var selectedPositions = new List<Position>();

        boardShips.ForEach(ship =>
            selectedPositions.AddRange(GetPositionsForShip(ship)));

        var hasDuplicates = selectedPositions
            .GroupBy(p => new { p.X, p.Y })
            .Any(g => g.Count() > 1);

        return hasDuplicates;
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
