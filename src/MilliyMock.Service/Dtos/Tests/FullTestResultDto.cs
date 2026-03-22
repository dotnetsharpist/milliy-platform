using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.Tests;

public class FullTestResultDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public List<QuestionAttemptDto> Questions { get; set; } = [];
    public List<QuestionGroupAttemptDto> QuestionGroups { get; set; } = [];
}