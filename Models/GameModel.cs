namespace Battleship.Models;

public class GameModel
{
    public required int GameId { get; set; }
    public string? Name { get; set; }
    public required UserModel User1 { get; set; }
    public required UserModel User2 { get; set; }
    public required UserModel ActiveUser { get; set; }
    public required int TurnCount { get; set; }
    public GameState state { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}

public enum GameState
{
    SETUP,
    PLAY,
    END
}
