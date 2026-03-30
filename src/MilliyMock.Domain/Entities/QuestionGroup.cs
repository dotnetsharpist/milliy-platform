using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class QuestionGroup : Auditable
{
    public string? Title { get; set; }
    public string? ImagePath { get; set; }
    public long TestId { get; set; }
    public Test Test { get; set; }
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Option> Options { get; set; } = new List<Option>();
}