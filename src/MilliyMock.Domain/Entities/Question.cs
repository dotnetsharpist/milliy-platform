using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class Question : Auditable
{
    public int Order { get; set; }
    public decimal Score { get; set; }
    public QuestionType Type { get; set; }
    public long TestId { get; set; }
    public Test Test { get; set; }
    public string? CorrectAnswer { get; set; }
    public long? QuestionGroupId { get; set; }
    public QuestionExplanation? QuestionExplanation { get; set; }
    public QuestionGroup? QuestionGroup { get; set; }
    public ICollection<Translation> Translations { get; set; } = new List<Translation>();
    public ICollection<Option> Options { get; set; } = new List<Option>();
}