namespace Battleship.Models;

public class ShipModel
{
    public required int ShipId { get; set; }
    public required string GameId { get; set; }
    public required ShipType Type { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required ShipOrientation Orientation { get; set; }
}
public enum ShipType
{
    CARRIER,
    BATTLESHIP,
    DESTROYER,
    SUBMARINE,
    PATROL_BOAT
}

public enum ShipOrientation
{
    HORIZONTAL,
    VERTICAL
}
