namespace MilliyMock.Service.Dtos.QuestionExplanations;

public class QuestionExplanationResultDto
{
    public long Id { get; set; }
    public string Text { get; set; } = null!;
    public string? ImagePath { get; set; }
}