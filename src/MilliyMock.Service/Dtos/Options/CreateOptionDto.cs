namespace MilliyMock.Service.Dtos.Options;

public class CreateOptionDto
{
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}