using Battleship.Controllers;

namespace Battleship.Models;

public class SetupViewModel
{
    public required Game Game { get; set; }
    public required List<ShipViewModel> Ships { get; set; }
}