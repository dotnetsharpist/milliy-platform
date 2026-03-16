namespace MilliyMock.Service.Dtos.Email;

public class EmailMessage
{
    public string Recipient { get; set; } = string.Empty;
    public string ConfirmationLink { get; set; } = string.Empty;
}