namespace MilliyMock.Service.Dtos.QuestionExplanations;

public class CreateQuestionExplanationDto
{
    public long QuestionId { get; set; }
    public string TextUz { get; set; } = null!;
    public string TextRu { get; set; } = null!;
    public string? VideoLink { get; set; }
}