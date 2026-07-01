namespace MilliyMock.Service.Dtos.Email;

public class EmailMessage
{
    public string Recipient { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 5;
}