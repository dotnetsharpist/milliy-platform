using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Dtos.UserTestAttempt;

public class UserTestAttemptResultDto
{
    public long UserId { get; set; }
    public UserResultDto User { get; set; } = null!;

    public long TestId { get; set; }
    public TestResultDto Test { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public decimal TotalScore { get; set; }

    public List<UserAnswerResultDto> UserAnswers { get; set; } = [];
}