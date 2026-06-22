using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.UserAnswers;

namespace MilliyMock.Service.Dtos.UserTestAttempt;

public class TestAttemptResultsDto
{
    public long TestAttemptId { get; set; }
    public string Title { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public List<QuestionAttemptResultDto> Questions { get; set; } = [];
    public List<QuestionGroupAttemptResultDto> QuestionGroups { get; set; } = [];
    public List<UserAnswerAttemptResultDto> UserAnswers { get; set; } = [];
}