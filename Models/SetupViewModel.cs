namespace Battleship.Models;

public class SetupViewModel
{
    public required Game Game { get; set; }
    public required List<ShipViewModel> Ships { get; set; }
    public required bool IsWaitingForOpponent { get; set; }
}
