namespace MilliyMock.Service.Dtos.UserTestAttempt;

public class CreateUserTestAttemptDto
{
    public long TestId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public decimal TotalScore { get; set; }
}