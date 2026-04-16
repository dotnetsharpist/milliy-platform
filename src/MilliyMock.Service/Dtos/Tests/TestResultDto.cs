using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Tests;

public class TestResultDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int QuestionCount { get; set; }
    public int AttemptCount { get; set; }
    public TestStatus Status { get; set; }
}