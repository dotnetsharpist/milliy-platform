namespace MilliyMock.Service.Dtos.Options;

public class CreateQuestionOptionDto // ts dto is for creating options for question
{
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}