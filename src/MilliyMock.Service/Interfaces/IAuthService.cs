using MilliyMock.Service.Dtos.Auth;
using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Interfaces;

public interface IAuthService
{
    ValueTask<string> Register(UserCreationDto dto);
    ValueTask<LoginResultDto> Login(LoginDto dto);
}