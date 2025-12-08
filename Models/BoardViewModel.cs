namespace Battleship.Models;

public class BoardViewModel
{
    public required Game Game { get; set; }
    public required List<ShipViewModel> MyShips { get; set; }
    public required List<ShipViewModel> OpponentSunkShips { get; set; }
    public required List<Shot> MyShots { get; set; }
    public required List<Shot> OpponentShots { get; set; }
}
