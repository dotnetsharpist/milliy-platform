namespace MilliyMock.Service.Dtos.UserAnswers;

public class CreateUserAnswer
{
    public long UserTestAttemptId { get; set; }
    public long QuestionId { get; set; }
    public long? SelectedOptionId { get; set; } 
    public string? TextAnswer { get; set; } 
}