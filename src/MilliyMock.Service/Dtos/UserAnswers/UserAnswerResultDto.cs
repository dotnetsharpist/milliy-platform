using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.UserTestAttempt;

namespace MilliyMock.Service.Dtos.UserAnswers;

public class UserAnswerResultDto
{
    public long UserTestAttemptId { get; set; }

    public long QuestionId { get; set; }
    public QuestionResultDto Question { get; set; } = null!;

    public long? SelectedOptionId { get; set; } 
    public OptionResultDto? SelectedOption { get; set; }

    public string? TextAnswer { get; set; } 
}