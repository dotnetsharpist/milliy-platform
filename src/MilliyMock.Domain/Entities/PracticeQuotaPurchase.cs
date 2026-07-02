using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class PracticeQuotaPurchase : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly Day { get; set; } // Tashkent-local date the purchase applies to
    public int ExtraQuestions { get; set; } // 10
    public int TangaSpent { get; set; } // 2
}
