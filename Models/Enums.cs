using NpgsqlTypes;

namespace Battleship.Models;

public enum GameState
{
    [PgName("SETUP")]
    SETUP,
    [PgName("PLAY")]
    PLAY,
    [PgName("END")]
    END
}

public enum ShipType
{
    [PgName("CARRIER")]
    CARRIER,
    [PgName("BATTLESHIP")]
    BATTLESHIP,
    [PgName("DESTROYER")]
    DESTROYER,
    [PgName("SUBMARINE")]
    SUBMARINE,
    [PgName("PATROL_BOAT")]
    PATROL_BOAT
}

public enum ShipOrientation
{
    [PgName("HORIZONTAL")]
    HORIZONTAL,
    [PgName("VERTICAL")]
    VERTICAL
}

public enum ShotOutcome
{
    [PgName("HIT")]
    HIT,
    [PgName("MISS")]
    MISS,
    [PgName("SINK")]
    SINK
}
