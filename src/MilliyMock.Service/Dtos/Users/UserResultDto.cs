using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Users;

public class UserResultDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string? GoogleId { get; set; }
    public long? BotUserId { get; set; }
    //public BotUser? BotUser { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}