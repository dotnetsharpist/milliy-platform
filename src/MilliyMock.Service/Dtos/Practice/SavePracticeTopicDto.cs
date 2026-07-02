using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Practice;

// Create/update payload for a topic (admin).
public class SavePracticeTopicDto
{
    public SubjectType Subject { get; set; }
    public string Slug { get; set; } = null!; // lowercase, "a-z 0-9 -" only
    public string Name { get; set; } = null!;
    public string? Section { get; set; }
    public int Order { get; set; }
}
