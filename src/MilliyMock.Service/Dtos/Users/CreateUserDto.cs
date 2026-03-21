namespace MilliyMock.Service.Dtos.Users;

public class CreateUserDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public long? BotUserId { get; set; }
}