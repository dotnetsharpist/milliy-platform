using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Questions;

public class QuestionAttemptForQuestionGroupDto
{
    public long Id { get; set; }
    public string? Text { get; set; }
    public string? ImagePath { get; set; }
    public int Order { get; set; }
    public int Score { get; set; }
    public QuestionType Type { get; set; }
    public long? QuestionGroupId { get; set; }
}