using MilliyMock.Service.Dtos.Auth;
using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Interfaces;

public interface IAuthService
{
    Task<string> Register(CreateUserDto dto);
    Task<LoginResultDto> TelegramLogin(TelegramLoginDto dto);
    ValueTask<LoginResultDto> Login(LoginDto dto);
}