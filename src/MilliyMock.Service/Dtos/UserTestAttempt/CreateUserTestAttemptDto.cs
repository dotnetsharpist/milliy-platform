namespace MilliyMock.Service.Dtos.UserTestAttempt;

public class CreateUserTestAttemptDto
{
    public long UserId { get; set; }
    public long TestId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int TotalScore { get; set; }
}