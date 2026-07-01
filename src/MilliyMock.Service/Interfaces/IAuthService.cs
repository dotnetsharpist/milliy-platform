using MilliyMock.Service.Dtos.Auth;
using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Interfaces;

public interface IAuthService
{
    Task<string> Register(RegisterDto dto);
    Task<LoginResultDto> VerifyEmail(VerifyEmailDto dto);
    Task<string> ResendOtp(ResendOtpDto dto);
    Task<LoginResultDto> TelegramLogin(TelegramLoginDto dto);
    Task<LoginResultDto> GoogleLogin(GoogleLoginDto dto);
    ValueTask<LoginResultDto> Login(LoginDto dto);
}