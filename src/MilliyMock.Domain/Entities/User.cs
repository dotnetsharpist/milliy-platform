using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class User : Auditable
{
    public required string FullName { get; set; }
    public required string Username { get; set; }
    public long? BotUserId { get; set; }
    public required string CurrentGrade { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public required string PasswordHash { get; set; }
    //public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
}
