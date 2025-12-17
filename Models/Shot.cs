namespace Battleship.Models;

public partial class Shot
{
    public int ShotId { get; set; }

    public int GameId { get; set; }

    public string ShooterUserId { get; set; } = null!;

    public int X { get; set; }

    public int Y { get; set; }

    public ShotOutcome Outcome { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User ShooterUser { get; set; } = null!;
}
