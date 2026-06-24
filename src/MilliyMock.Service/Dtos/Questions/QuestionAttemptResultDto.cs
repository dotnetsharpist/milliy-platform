using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Options;

namespace MilliyMock.Service.Dtos.Questions;

public class QuestionAttemptResultDto
{
    public long Id { get; set; }
    public int Order { get; set; }
    public decimal Score { get; set; }
    public QuestionType Type { get; set; }
    public long? QuestionGroupId { get; set; }
    public string? CorrectAnswer { get; set; }
    public List<TranslationResultDto> Translations { get; set; } = [];
    public List<OptionAttemptResultDto> Options { get; set; } = [];
}
