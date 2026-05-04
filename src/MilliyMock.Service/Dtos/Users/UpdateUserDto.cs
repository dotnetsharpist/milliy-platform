namespace MilliyMock.Service.Dtos.Users;

public class UpdateUserDto
{
    public string FullName { get; set; } = null!;
    public string? PasswordHash { get; set; }
}