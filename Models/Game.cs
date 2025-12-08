namespace Battleship;

public partial class Game
{
    public int GameId { get; set; }

    public string? Name { get; set; }

    public int User1Id { get; set; }

    public int User2Id { get; set; }

    public int ActiveUserId { get; set; }

    public int TurnCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual User ActiveUser { get; set; } = null!;

    public virtual ICollection<Ship> Ships { get; set; } = new List<Ship>();

    public virtual ICollection<Shot> Shots { get; set; } = new List<Shot>();

    public virtual User User1 { get; set; } = null!;

    public virtual User User2 { get; set; } = null!;
}
