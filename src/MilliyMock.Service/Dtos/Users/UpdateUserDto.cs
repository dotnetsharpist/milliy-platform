namespace MilliyMock.Service.Dtos.Users;

public class UpdateUserDto
{
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public string? PhoneNumber { get; set; }
}