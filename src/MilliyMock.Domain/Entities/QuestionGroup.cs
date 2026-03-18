using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

public class QuestionGroup : Auditable
{
    public string? Title { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();

    public ICollection<Option> Options { get; set; } = new List<Option>();
}