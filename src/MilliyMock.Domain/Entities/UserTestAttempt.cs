using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class UserTestAttempt : Auditable
{
    public long? UserId { get; set; }
    public User? User { get; set; }
    
    public long? TempUserId { get; set; }
    public TempUser? TempUser { get; set; }
    public long TestId { get; set; }
    public Test Test { get; set; } = null!;
    public AttemptStatus AttemptStatus { get; set; } = AttemptStatus.InProgress;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public decimal TotalScore { get; set; }
    public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}