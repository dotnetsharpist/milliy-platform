using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Options;

namespace MilliyMock.Service.Dtos.Questions;

public class QuestionAttemptDto
{
    public long Id { get; set; }
    public string? Text { get; set; }
    public string? ImagePath { get; set; }
    public int Order { get; set; }
    public int Score { get; set; }
    public QuestionType Type { get; set; }
    public long? QuestionGroupId { get; set; }
    public List<OptionAttemptDto> Options { get; set; } = [];
}