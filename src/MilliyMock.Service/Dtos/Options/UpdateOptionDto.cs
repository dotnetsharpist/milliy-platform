namespace MilliyMock.Service.Dtos.Options;

public class UpdateOptionDto
{
    public string Text { get; set; } = null!;
    public long? QuestionId { get; set; }
    public long? QuestionGroupId { get; set; }
    public bool IsCorrect { get; set; }
}

