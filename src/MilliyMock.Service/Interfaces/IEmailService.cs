using MilliyMock.Service.Dtos.Email;

namespace MilliyMock.Service.Interfaces;

public interface IEmailService
{
    public Task<bool> SendAsync(EmailMessage emailMessage);
}