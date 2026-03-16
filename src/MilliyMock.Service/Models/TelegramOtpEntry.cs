namespace MilliyMock.Service.Models;

public class TelegramOtpEntry
{
    public long UserId { get; set; }
    public string Code { get; set; }
    public DateTime CreatedAt { get; set; }
}