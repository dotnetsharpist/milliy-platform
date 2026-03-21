namespace MilliyMock.Service.Dtos.Options;

public class CreateOptionDto
{
    public long QuestionId { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}