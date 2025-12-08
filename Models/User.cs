namespace Battleship.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public virtual ICollection<Game> GameActiveUsers { get; set; } = new List<Game>();

    public virtual ICollection<Game> GameUser1s { get; set; } = new List<Game>();

    public virtual ICollection<Game> GameUser2s { get; set; } = new List<Game>();

    public virtual ICollection<Shot> Shots { get; set; } = new List<Shot>();

    public virtual ICollection<Ship> Ships { get; set; } = new List<Ship>();
}
