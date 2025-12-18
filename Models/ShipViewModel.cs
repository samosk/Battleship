using System.ComponentModel.DataAnnotations;
using Battleship.Controllers;

namespace Battleship.Models;

public class ShipViewModel
{
    public int X { get; set; }
    public int Y { get; set; }
    public required ShipOrientation Orientation { get; set; }
    public required ShipType Type { get; set; }
    public required bool IsMine { get; set; }
    public required bool IsSunk { get; set; }

    public int Length => GameController.GetShipLength(this);
    public int StartColumn => X;
    public int EndColumn => Orientation == ShipOrientation.HORIZONTAL ? StartColumn + Length : StartColumn + 1;
    public int StartRow => Y;
    public int EndRow => Orientation == ShipOrientation.VERTICAL ? StartRow + Length : StartRow + 1;
}
