using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class Option : Auditable
{
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; } = false;
    public required long QuestionId { get; set; }
    public Question Question { get; set; }
}