using MilliyMock.Service.Dtos.QuestionGroups;

namespace MilliyMock.Service.Dtos.Questions;

public class CreateManyQuestionsDto
{
    public int TestId { get; set; }
    public List<CreateQuestionDto> Questions { get; set; } = new();
    public List<CreateQuestionGroupDto>? QuestionGroups { get; set; } = new();
}