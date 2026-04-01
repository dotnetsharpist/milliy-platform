using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class Test : Auditable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}