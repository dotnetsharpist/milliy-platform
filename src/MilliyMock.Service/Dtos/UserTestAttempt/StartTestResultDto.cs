using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.UserTestAttempt;

public class StartTestResultDto
{
    public long TestAttemptId { get; set; }
    public string Title { get; set; } = null!;
    public List<QuestionAttemptDto> Questions { get; set; } = [];
    public List<QuestionGroupAttemptDto> QuestionGroups { get; set; } = [];
}