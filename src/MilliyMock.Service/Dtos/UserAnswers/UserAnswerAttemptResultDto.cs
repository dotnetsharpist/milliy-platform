namespace MilliyMock.Service.Dtos.UserAnswers;

public class UserAnswerAttemptResultDto
{
    public long QuestionId { get; set; }
    public long? SelectedOptionId { get; set; }
    public string? TextAnswer { get; set; }
}
