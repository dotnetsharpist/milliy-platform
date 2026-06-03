using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Tests;

public class CreateTestDto
{
    public string Title { get; set; } = null!;
    public SubjectType Subject { get; set; } = SubjectType.Math;
    public string? Description { get; set; }
}