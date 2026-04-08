namespace MilliyMock.Service.Dtos.Options;

public class UpdateQuestionOptionDto
{
    public long? Id { get; set; }       // null = new option, non-null = existing option to update
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

