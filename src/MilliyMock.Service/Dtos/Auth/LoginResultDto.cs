using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Dtos.Auth;

public class LoginResultDto
{
    public string Token { get; set; }
    public UserResultDto User { get; set; }
}