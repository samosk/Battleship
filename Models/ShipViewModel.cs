namespace Battleship.Models;

public class ShipViewModel
{
    public int X { get; set; }
    public int Y { get; set; }
    public required ShipOrientation Orientation { get; set; }
    public required ShipType Type { get; set; }
    public required bool IsMine { get; set; }
    public required bool IsSunk { get; set; }
}
