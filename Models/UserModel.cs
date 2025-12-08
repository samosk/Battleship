namespace Battleship.Models;

public class UserModel
{
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public string? PasswordHash { get; set; }
}
