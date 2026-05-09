using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.QuestionExplanations;

public class QuestionExplanationResultDto
{
    public long Id { get; set; }
    public string? VideoLink { get; set; }

    public List<TranslationResultDto> Translations { get; set; } = [];
}