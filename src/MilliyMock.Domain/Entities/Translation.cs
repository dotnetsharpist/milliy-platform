using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class Translation : Auditable
{
    public Language Language { get; set; } = Language.Uzbek;
    public string Text { get; set; } = null!;
    public string? ImagePath { get; set; }
    public long? QuestionId { get; set; }
    public Question? Question { get; set; }
    public long? QuestionGroupId { get; set; }
    public QuestionGroup? QuestionGroup { get; set; }
    public long? QuestionExplanationId { get; set; }
    public QuestionExplanation? QuestionExplanation { get; set; }
}