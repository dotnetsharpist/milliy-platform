using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Users;

public class UserResultDto
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public long? BotUserId { get; set; }
    //public BotUser? BotUser { get; set; }
    public UserRole Role { get; set; }
}