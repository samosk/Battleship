namespace Battleship.Models;

public partial class Ship
{
    public int ShipId { get; set; }

    public int GameId { get; set; }

    public string UserId { get; set; } = null!;

    public int X { get; set; }

    public int Y { get; set; }

    public ShipOrientation Orientation { get; set; }

    public ShipType Type { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public Ship()
    { }

    public Ship(ShipViewModel vm)
    {
        X = vm.X;
        Y = vm.Y;
        Orientation = vm.Orientation;
        Type = vm.Type;
    }
}
