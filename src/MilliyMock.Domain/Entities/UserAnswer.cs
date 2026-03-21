using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class UserAnswer : Auditable
{
    public long UserTestAttemptId { get; set; }
    public UserTestAttempt UserTestAttempt { get; set; } = null!;
    public long QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public long? SelectedOptionId { get; set; } 
    public Option? SelectedOption { get; set; }
    public string? TextAnswer { get; set; } 
}