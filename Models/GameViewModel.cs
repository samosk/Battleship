using System.ComponentModel.DataAnnotations;
using Battleship.Controllers;

namespace Battleship.Models;

public class GameViewModel
{
    public int GameId { get; set; }

    public string? Name { get; set; }

    public User? User1 { get; set; }

    public string? User1Id { get; set; }

    public User? User2 { get; set; }

    public string? User2Id { get; set; }

    public bool IsMyTurn { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public GameState State { get; set; }
}
