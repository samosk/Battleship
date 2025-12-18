using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
    [Display(Name = "Carrier")]
    [PgName("CARRIER")]
    CARRIER,

    [Display(Name = "Battleship")]
    [PgName("BATTLESHIP")]
    BATTLESHIP,

    [Display(Name = "Destroyer")]
    [PgName("DESTROYER")]
    DESTROYER,

    [Display(Name = "Submarine")]
    [PgName("SUBMARINE")]
    SUBMARINE,

    [Display(Name = "Patrol Boat")]
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

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()
            ?.GetName() ?? enumValue.ToString();
    }
}