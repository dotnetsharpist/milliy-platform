using System.Net;
using System.Net.Mail;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Email;
using MilliyMock.Service.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MilliyMock.Service.Services;

public class EmailService(IConfiguration configuration): IEmailService
{
    private readonly string Email = configuration["Email:Sender"];
    private readonly string Host = configuration["Email:BaseURL"];
    private readonly string Password = configuration["Email:Password"];
    private readonly int Port = 587;
    
    public async Task<bool> SendAsync(EmailMessage emailMessage)
    {
        try
        {
            using var client = new SmtpClient(Host, Port);
            client.Credentials = new NetworkCredential(Email, Password);
            client.EnableSsl = true;
            
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Templates",
                "confirm-email.html");
            
            var htmlBody = await File.ReadAllTextAsync(templatePath);
            htmlBody = htmlBody.Replace("{{VERIFY_LINK}}", emailMessage.ConfirmationLink);

            var message = new MailMessage
            {
                From = new MailAddress(Email, Email),
                Subject = "Hello",
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(emailMessage.Recipient);

            try
            {
                await client.SendMailAsync(message);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw new MilliyMockException();
            }

            return true;
        }
        catch
        {
            throw new MilliyMockException();
        }
    }
}