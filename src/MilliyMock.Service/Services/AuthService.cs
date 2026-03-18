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

    public async ValueTask<string> Register(UserCreationDto dto)
    {
        if (HttpContextHelper.UserId != null)
            throw new MilliyMockException(400, "Bad request.");
        
        var existingUser = await unitOfWork.Users.SelectAsync(u => u.Email == dto.Email);
        if (existingUser != null) throw new MilliyMockException(409, "User with this email already exists!");

        var mappedUser = mapper.Map<User>(dto);
        mappedUser.PasswordHash = PasswordHelper.Hash(dto.Password);
        await unitOfWork.Users.InsertAsync(mappedUser);
        if (!await unitOfWork.Users.SaveAsync())
            throw new MilliyMockException();

        var otp = OtpGenerator.GenerateOtp();
        var otpModel = new TelegramOtpEntry()
        {
            UserId = mappedUser.Id,
            Code = otp,
            CreatedAt = DateTime.UtcNow
        };

        memoryCache.Set(
            $"otp_{otp}",
            otpModel,
            TimeSpan.FromMinutes(10)
        );        //await emailService.SendAsync(emailMessage);
        return otp;
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