using System.ComponentModel.DataAnnotations.Schema;
using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class User : Auditable
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; } = false;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public long? BotUserId { get; set; }
    public BotUser? BotUser { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}