using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Google.Apis.Auth;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Auth;
using MilliyMock.Service.Dtos.Users;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using MilliyMock.Service.Dtos.Email;
using MilliyMock.Service.Models;


namespace MilliyMock.Service.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IMapper mapper,
    ILogger<AuthService> logger,
    IMemoryCache memoryCache,
    IConfiguration configuration) : IAuthService
{
    private readonly IConfiguration _configuration = configuration.GetSection("Jwt");

    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromMinutes(1);

    public async Task<string> Register(RegisterDto dto)
    {
        try
        {
            logger.LogInformation("Registration attempt for email {email}", dto.Email);

            var existing = await unitOfWork.Users.SelectAsync(u => u.Email == dto.Email);
            if (existing is not null)
            {
                if (existing.EmailConfirmed)
                    throw new MilliyMockException(409, "This email is already registered.");

                // A previous, never-verified attempt — refresh the details and re-issue
                // a code (cooldown-guarded) instead of creating a duplicate account.
                EnsureCooldownElapsed(existing.Email!);

                existing.FirstName = dto.FirstName;
                existing.LastName = dto.LastName;
                existing.FatherName = dto.FatherName;
                existing.PasswordHash = PasswordHelper.Hash(dto.Password);
                unitOfWork.Users.Update(existing);
                await unitOfWork.SaveChangesAsync();

                await IssueAndSendOtpAsync(existing);
                return "A new verification code has been sent to your email.";
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FatherName = dto.FatherName,
                Email = dto.Email,
                PasswordHash = PasswordHelper.Hash(dto.Password),
                EmailConfirmed = false
            };
            await unitOfWork.Users.InsertAsync(user);
            await unitOfWork.SaveChangesAsync();

            await IssueAndSendOtpAsync(user);
            return "A verification code has been sent to your email.";
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during registration for email {email}", dto.Email);
            throw new MilliyMockException();
        }
    }

    public async Task<LoginResultDto> VerifyEmail(VerifyEmailDto dto)
    {
        try
        {
            logger.LogInformation("Email verification attempt for email {email}", dto.Email);

            if (!memoryCache.TryGetValue(OtpCacheKey(dto.Email), out OtpEntry? entry) || entry is null)
                throw new MilliyMockException(400,
                    "The verification code has expired or was never requested. Please request a new one.");

            if (entry.Code != dto.Code)
                throw new MilliyMockException(400, "Invalid verification code");

            var user = await unitOfWork.Users.SelectAsync(u => u.Id == entry.UserId);
            if (user is null) throw new MilliyMockException(404, "User not found");

            user.EmailConfirmed = true;
            unitOfWork.Users.Update(user);
            await unitOfWork.SaveChangesAsync();

            memoryCache.Remove(OtpCacheKey(dto.Email));

            return new LoginResultDto
            {
                Token = GenerateToken(user),
                User = mapper.Map<UserResultDto>(user)
            };
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during email verification for email {email}", dto.Email);
            throw new MilliyMockException();
        }
    }

    public async Task<string> ResendOtp(ResendOtpDto dto)
    {
        try
        {
            logger.LogInformation("OTP resend attempt for email {email}", dto.Email);

            var user = await unitOfWork.Users.SelectAsync(u => u.Email == dto.Email);
            if (user is null) throw new MilliyMockException(404, "User not found");
            if (user.EmailConfirmed) throw new MilliyMockException(409, "This email is already verified.");

            EnsureCooldownElapsed(user.Email!);

            await IssueAndSendOtpAsync(user);
            return "A new verification code has been sent to your email.";
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during otp resend for email {email}", dto.Email);
            throw new MilliyMockException();
        }
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

    public async Task<LoginResultDto> GoogleLogin(GoogleLoginDto dto)
    {
        try
        {
            var clientId = configuration["Google:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                throw new MilliyMockException(500, "Google client id is not configured");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });
            }
            catch (InvalidJwtException ex)
            {
                logger.LogWarning(ex, "Invalid Google token");
                throw new MilliyMockException(401, "Invalid Google token");
            }

            logger.LogInformation("Login attempt via google with email {email}", payload.Email);

            // Already linked to this Google account → straight login.
            var user = await unitOfWork.Users.SelectAsync(u => u.GoogleId == payload.Subject);

            if (user is null)
            {
                // The email might already belong to an account (email/password, telegram…).
                var existing = await unitOfWork.Users.SelectAsync(u => u.Email == payload.Email);
                if (existing is not null)
                {
                    // Only attach Google to it when Google has verified the address —
                    // otherwise a forged unverified email could hijack someone's account.
                    if (!payload.EmailVerified)
                        throw new MilliyMockException(409,
                            "An account with this email already exists. Sign in with your password.");

                    existing.GoogleId = payload.Subject;
                    existing.EmailConfirmed = true;
                    user = existing;
                }
                else
                {
                    user = new User
                    {
                        FirstName = payload.GivenName ?? payload.Name ?? "",
                        LastName = payload.FamilyName,
                        Email = payload.Email,
                        EmailConfirmed = payload.EmailVerified,
                        GoogleId = payload.Subject
                    };
                    await unitOfWork.Users.InsertAsync(user);
                }

                await unitOfWork.SaveChangesAsync();
            }

            return new LoginResultDto
            {
                Token = GenerateToken(user),
                User = mapper.Map<UserResultDto>(user)
            };
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during google login");
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

            if (!user.EmailConfirmed)
                throw new MilliyMockException(403, "Please verify your email before logging in.");

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

    private async Task IssueAndSendOtpAsync(User user)
    {
        var code = GenerateOtp();
        memoryCache.Set(
            OtpCacheKey(user.Email!),
            new OtpEntry(user.Id, code, TimeHelper.GetDateTime()),
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = OtpLifetime });

        await emailService.SendRegisterOtpAsync(new EmailMessage
        {
            Recipient = user.Email!,
            OtpCode = code,
            FirstName = user.FirstName,
            ExpiryMinutes = (int)OtpLifetime.TotalMinutes
        });
    }

    private void EnsureCooldownElapsed(string email)
    {
        if (memoryCache.TryGetValue(OtpCacheKey(email), out OtpEntry? entry) && entry is not null)
        {
            var elapsed = TimeHelper.GetDateTime() - entry.SentAt;
            if (elapsed < OtpResendCooldown)
            {
                var secondsLeft = (int)Math.Ceiling((OtpResendCooldown - elapsed).TotalSeconds);
                throw new MilliyMockException(429,
                    $"Please wait {secondsLeft} seconds before requesting a new code.");
            }
        }
    }

    private static string GenerateOtp() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string OtpCacheKey(string email) => $"email-otp:{email.Trim().ToLowerInvariant()}";

    private sealed record OtpEntry(long UserId, string Code, DateTime SentAt);
}