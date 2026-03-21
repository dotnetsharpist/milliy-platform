using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class Question : Auditable
{
    public string? Text { get; set; }
    public string? ImagePath { get; set; }
    public int Order { get; set; }
    public int Score { get; set; }
    public QuestionType Type { get; set; }
    public long TestId { get; set; }
    public Test Test { get; set; }
    public string? CorrectAnswer { get; set; }
    public long? QuestionGroupId { get; set; }
    public QuestionGroup? QuestionGroup { get; set; }

    public ICollection<Option> Options { get; set; } = new List<Option>();
}