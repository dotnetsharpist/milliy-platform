using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.QuestionExplanations;

namespace MilliyMock.Service.Dtos.Questions;

public class QuestionResultDto
{
    public long Id { get; set; }
    public int Order { get; set; }
    public decimal Score { get; set; }
    public QuestionType Type { get; set; }
    public long TestId { get; set; }
    public string? CorrectAnswer { get; set; }
    public long? QuestionGroupId { get; set; }

    public QuestionExplanationResultDto QuestionExplanation { get; set; } = new();
    public List<TranslationResultDto> Translations { get; set; } = new();
    public List<OptionResultDto> Options { get; set; } = new();
}