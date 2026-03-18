using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class UserTestAttempt : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long TestId { get; set; }
    public Test Test { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public int TotalScore { get; set; }

    public ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
}