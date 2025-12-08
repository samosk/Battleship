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
    CRUISER,
    SUBMARINE,
    DESTROYER
}

public enum ShipOrientation
{
    HORIZONTAL,
    VERTICAL
}

public enum ShotOutcome
{
    MISS,
    HIT,
    SINK
}
