using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Auth;
using MilliyMock.Service.Dtos.Users;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;


namespace MilliyMock.Service.Services;

public class AuthService(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    ILogger<AuthService> logger,
    IConfiguration configuration) : IAuthService
{
    private readonly IConfiguration _configuration = configuration.GetSection("Jwt");

    public Task<string> Register(CreateUserDto dto)
    {
        throw new MilliyMockException();
    }

    public async Task<LoginResultDto> TelegramLogin(TelegramLoginDto dto)
    {
        try
        {
            var authDate = DateTimeOffset.FromUnixTimeSeconds(dto.AuthDate);
            if (DateTimeOffset.UtcNow - authDate > TimeSpan.FromMinutes(5))
                throw new MilliyMockException(409, "Expired");

            if (!ValidateTelegramData(dto)) throw new MilliyMockException(409, "Bruh");

            logger.LogInformation("Login attempt via telegram with username {username}", dto.Username);
            var user = await unitOfWork.Users.SelectAsync(u => u.BotUser != null && u.BotUser.TgUserId == dto.Id);

            if (user is not null)
                return new LoginResultDto
                {
                    Token = GenerateToken(user),
                    User = mapper.Map<UserResultDto>(user)
                };

            // Reuse an existing BotUser (e.g. one the bot created when this person
            // messaged it first). Inserting a new one would violate the unique
            // TgUserId index and fail the whole login.
            var botUser = await unitOfWork.BotUsers.SelectAsync(bu => bu.TgUserId == dto.Id);
            if (botUser is null)
            {
                botUser = new BotUser
                {
                    TgUserId = dto.Id,
                    Username = dto.Username,
                    FullName = $"{dto.FirstName} {dto.LastName}".Trim()
                };
                await unitOfWork.BotUsers.InsertAsync(botUser);
            }
            else
            {
                // Adopting an orphan the bot created earlier — refresh its profile
                // from the login payload, which is the more up-to-date source.
                botUser.Username = dto.Username;
                botUser.FullName = $"{dto.FirstName} {dto.LastName}".Trim();
            }

            var newUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName ?? "",
                BotUser = botUser
            };
            await unitOfWork.Users.InsertAsync(newUser);

            await unitOfWork.SaveChangesAsync();

            return new LoginResultDto
            {
                Token = GenerateToken(newUser),
                User = mapper.Map<UserResultDto>(newUser)
            };
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during telegram login for username {username}", dto.Username);
            throw new MilliyMockException();
        }
    }

    public async ValueTask<LoginResultDto> Login(LoginDto dto)
    {
        try
        {
            logger.LogInformation("Login attempt for email {email}", dto.Email);
            var user = await unitOfWork.Users.SelectAsync(u => u.Email == dto.Email);
            if (user == null) throw new MilliyMockException(404, "User not found");

            if (!PasswordHelper.Verify(dto.Password, user.PasswordHash))
                throw new MilliyMockException(400, "Password or email is incorrect");

            return new LoginResultDto
            {
                Token = GenerateToken(user),
                User = mapper.Map<UserResultDto>(user)
            };
        }
        catch (MilliyMockException ex)
        {
            logger.LogWarning(ex, "Login failed for email {email} with message: {message}", dto.Email, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login for email {email}", dto.Email);
            throw new MilliyMockException();
        }
    }
    
    private string GenerateToken(User user)
    {
        var identityClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim("Purpose", "accessToken"),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecurityKey"]!));
        var keyCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expireDays = int.Parse(_configuration["Lifetime"]!);
        var token = new JwtSecurityToken(
            issuer: _configuration["Issuer"],
            audience: _configuration["Audience"],
            claims: identityClaims,
            expires: TimeHelper.GetDateTime().AddDays(expireDays),
            signingCredentials: keyCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private bool ValidateTelegramData(TelegramLoginDto dto)
    {
        var botToken = configuration.GetValue<string>("BotConfiguration:BotToken");
        var dataCheckDict = new SortedDictionary<string, string>
        {
            { "auth_date", dto.AuthDate.ToString() },
            { "first_name", dto.FirstName ?? "" },
            { "id", dto.Id.ToString() },
            { "username", dto.Username ?? "" }
        };

        if (!string.IsNullOrEmpty(dto.LastName))
            dataCheckDict.Add("last_name", dto.LastName);

        if (!string.IsNullOrEmpty(dto.PhotoUrl))
            dataCheckDict.Add("photo_url", dto.PhotoUrl);

        var dataCheckString = string.Join("\n",
            dataCheckDict.Select(kvp => $"{kvp.Key}={kvp.Value}")
        );

        using var sha256 = SHA256.Create();
        var secretKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(botToken));

        using var hmac = new HMACSHA256(secretKey);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
        var computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return computedHash == dto.Hash;
    }
}