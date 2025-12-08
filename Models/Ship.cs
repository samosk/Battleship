namespace Battleship;

public partial class Ship
{
    public int ShipId { get; set; }

    public int GameId { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public virtual Game Game { get; set; } = null!;
}
