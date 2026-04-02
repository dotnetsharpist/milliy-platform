namespace MilliyMock.Service.Dtos.Options;

public class UpdateQuestionOptionDto
{
    /// <summary>
    /// Id of an existing option to update. Leave 0 (or omit) to create a new option.
    /// </summary>
    public long Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}

