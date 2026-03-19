namespace MilliyMock.Service.Dtos.Users;

public class CreateUserDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string CurrentGrade { get; set; }
    //public UserMajor Major { get; set; }
    public string Password { get; set; }
    public bool WantsOpportunityNotifications { get; set; } = false;
}