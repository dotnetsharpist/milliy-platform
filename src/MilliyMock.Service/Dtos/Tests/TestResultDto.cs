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
    public SubjectType Subject { get; set; }
    public bool IsPremium { get; set; }
    public int Price { get; set; }

    /// <summary>
    /// True when the current user can open the full test:
    /// the test is free, the user already purchased it, or the user is an admin.
    /// </summary>
    public bool IsPurchased { get; set; }
}