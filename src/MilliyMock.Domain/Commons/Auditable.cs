using MilliyMock.Shared.Helpers;

namespace MilliyMock.Domain.Commons;

public class Auditable
{
    public long Id { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = TimeHelper.GetDateTime();
    public DateTime? UpdatedAt { get; set; }
}