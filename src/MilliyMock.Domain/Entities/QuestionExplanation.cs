using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class QuestionExplanation : Auditable
{
    public string Text { get; set; } = null!;
    public string? ImagePath { get; set; }
    public long? QuestionId { get; set; }
    public Question? Question { get; set; }
    public long? QuestionGroupId { get; set; }
    public QuestionGroup? QuestionGroup { get; set; }
    
    public ICollection<Translation> Translations { get; set; } = new List<Translation>();
}