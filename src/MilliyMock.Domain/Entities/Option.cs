using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class Option : Auditable
{
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; } = false;
    public long? QuestionId { get; set; }
    public Question? Question { get; set; }
    public long? QuestionGroupId { get; set; }
    public QuestionGroup? QuestionGroup { get; set; }
}