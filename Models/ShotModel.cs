namespace Battleship.Models;

public class ShotModel
{
    public required int ShotId { get; set; }
    public required string GameId { get; set; }
    public required string ShooterUserId { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required ShotOutcome Outcome { get; set; }
}

public enum ShotOutcome
{
    MISS,
    HIT,
    SINK
}
