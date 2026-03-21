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
using Microsoft.IdentityModel.Tokens;

namespace MilliyMock.Service.Services;

public class AuthService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IMapper mapper, IConfiguration configuration) : IAuthService
{
    private readonly IConfiguration _configuration = configuration.GetSection("Jwt");

    public ValueTask<string> Register(CreateUserDto dto)
    {
        throw new MilliyMockException();
    }
    
    public async ValueTask<LoginResultDto> Login(LoginDto dto)
    {
        var user = await unitOfWork.Users.SelectAsync(u => u.Email == dto.Email);
        if (user == null) throw new MilliyMockException(404, "User not found");

        if (!PasswordHelper.Verify(dto.Password, user.PasswordHash))
            throw new MilliyMockException(409, "Password or email is incorrect");

        return new LoginResultDto
        {
            Token = GenerateToken(user)
        };
    }
    
    private string GenerateToken(User user)
    {
        var identityClaims = new[]
        {
            new Claim("Id", user.Id.ToString()),
            new Claim("FullName", user.FullName),
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