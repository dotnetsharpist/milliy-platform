using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class QuestionGroupAttemptDto
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public List<QuestionAttemptDto> Questions { get; set; } = [];
    public List<QuestionAttemptForQuestionGroupDto> Options { get; set; } = [];
}