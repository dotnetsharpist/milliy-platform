using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Tests;

public class UpdateTestDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TestStatus? Status { get; set; }
    public SubjectType? Subject { get; set; }
}