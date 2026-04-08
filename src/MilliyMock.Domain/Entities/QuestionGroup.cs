using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class QuestionGroup : Auditable
{
    public long TestId { get; set; }
    public Test Test { get; set; }
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Translation> Translations { get; set; } = new List<Translation>();
    public ICollection<Option> Options { get; set; } = new List<Option>();
}