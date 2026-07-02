using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class PracticeAttempt : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long PracticeQuestionId { get; set; }
    public PracticeQuestion PracticeQuestion { get; set; } = null!;
    public string ChosenLetter { get; set; } = null!; // "A".."D"
    public bool IsCorrect { get; set; } // denormalized for fast status filtering
    // answer timestamp = Auditable.CreatedAt
}
