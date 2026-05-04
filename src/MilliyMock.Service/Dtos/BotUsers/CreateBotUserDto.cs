namespace MilliyMock.Service.Dtos.BotUsers;

public class CreateBotUserDto
{
    public long TgUserId { get; set; }
    public string? Username { get; set; } 
    public string FullName { get; set; } = null!;
}