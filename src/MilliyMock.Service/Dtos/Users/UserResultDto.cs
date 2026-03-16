namespace MilliyMock.Service.Dtos.Users;

public class UserResultDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string CurrentGrade { get; set; } // "11", "12", "Undergraduate", "Gap Year"
    public required string Major { get; set; }
    public bool WantsOpportunityNotifications { get; set; } = true;
}