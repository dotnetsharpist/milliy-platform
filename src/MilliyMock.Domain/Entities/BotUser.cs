using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class BotUser : Auditable
{
    public long TgUserId { get; set; }
    public string? Username { get; set; } 
    public string FullName { get; set; } = null!;
}