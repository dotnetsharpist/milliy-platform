using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.QuestionGroups;

namespace MilliyMock.Service.Dtos.Questions;

public class CreateManyQuestionsResultDto
{
    public int SuccessCount => CreatedQuestions.Count + CreatedQuestionGroups.Count;
    public int FailCount { get; set; }
    public List<QuestionGroupResultDto> CreatedQuestionGroups { get; set; } = new();
    public List<QuestionResultDto> CreatedQuestions { get; set; } = new();
}
