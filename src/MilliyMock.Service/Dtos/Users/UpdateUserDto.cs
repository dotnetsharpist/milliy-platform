namespace MilliyMock.Service.Dtos.Users;

public class UpdateUserDto
{
    public string FullName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public string? PasswordHash { get; set; }
}