namespace Battleship.Models;

public enum GameState
{
    SETUP,
    PLAY,
    END
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

public enum ShotOutcome
{
    HIT,
    MISS,
    SINK
}
