using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class UserBalance : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } 
    public int Balance { get; set; }
}