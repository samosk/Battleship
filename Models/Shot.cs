namespace Battleship;

public partial class Shot
{
    public int ShotId { get; set; }

    public int GameId { get; set; }

    public int ShooterUserId { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User ShooterUser { get; set; } = null!;
}
