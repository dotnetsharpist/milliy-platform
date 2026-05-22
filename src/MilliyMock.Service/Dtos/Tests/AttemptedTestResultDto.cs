using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.TempUsers;
using MilliyMock.Service.Dtos.UserAnswers;

namespace MilliyMock.Service.Dtos.Tests;

public class AttemptedTestResultDto
{
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public TempUserResultDto? TempUser { get; set; }
    public List<UserAnswerResultDto> UserAnswers { get; set; }
    public List<QuestionResultDto> Questions { get; set; }
}