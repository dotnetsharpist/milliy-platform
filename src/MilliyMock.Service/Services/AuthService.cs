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
using MilliyMock.Service.Models;
using MilliyMock.Shared.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MilliyMock.Service.Services;

public class AuthService(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    ILogger<AuthService> logger,
    IConfiguration configuration) : IAuthService
{
    private readonly IConfiguration _configuration = configuration.GetSection("Jwt");

    public ValueTask<string> Register(CreateUserDto dto)
    {
        throw new MilliyMockException();
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
            new Claim(ClaimTypes.Name, user.FullName),
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
}