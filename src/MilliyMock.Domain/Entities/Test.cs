using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class Test : Auditable
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TestStatus Status { get; set; } = TestStatus.Draft;
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuestionGroup> QuestionGroups { get; set; } = new List<QuestionGroup>();
}