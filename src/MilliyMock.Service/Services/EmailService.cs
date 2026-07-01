using System.Net;
using System.Net.Mail;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Email;
using MilliyMock.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MilliyMock.Service.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger): IEmailService
{
    private readonly string Email = configuration["Email:Sender"];
    private readonly string Host = configuration["Email:BaseURL"];
    private readonly string Password = configuration["Email:Password"];
    private readonly int Port = 587;
    
    public async Task<bool> SendRegisterOtpAsync(EmailMessage emailMessage)
    {
        try
        {
            using var client = new SmtpClient(Host, Port);
            client.Credentials = new NetworkCredential(Email, Password);
            client.EnableSsl = true;
            
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Templates",
                "otp-verification-email.html");
            
            var htmlBody = await File.ReadAllTextAsync(templatePath);
            htmlBody = htmlBody.Replace("{{FIRST_NAME}}", emailMessage.FirstName);
            htmlBody = htmlBody.Replace("{{OTP_CODE}}", emailMessage.OtpCode);
            htmlBody = htmlBody.Replace("{{EXPIRY_MINUTES}}", emailMessage.ExpiryMinutes.ToString());

            var message = new MailMessage
            {
                From = new MailAddress(Email, Email),
                Subject = "Salom",
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