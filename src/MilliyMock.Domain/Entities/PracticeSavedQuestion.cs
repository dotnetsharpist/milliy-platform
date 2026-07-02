using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class PracticeSavedQuestion : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long PracticeQuestionId { get; set; }
    public PracticeQuestion PracticeQuestion { get; set; } = null!;
}
