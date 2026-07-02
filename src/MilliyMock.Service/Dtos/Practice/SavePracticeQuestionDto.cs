using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Practice;

// Create/update payload for a question (admin). Topic must reference an
// existing PracticeTopic slug of the same subject.
public class SavePracticeQuestionDto
{
    public SubjectType Subject { get; set; }
    public int? Grade { get; set; } // 5..11 or null = any grade
    public PracticeDifficulty Difficulty { get; set; }
    public string Topic { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string OptionA { get; set; } = null!;
    public string OptionB { get; set; } = null!;
    public string OptionC { get; set; } = null!;
    public string OptionD { get; set; } = null!;
    public string CorrectLetter { get; set; } = null!; // "A".."D"
    public string? ExplanationTitle { get; set; }
    public string? Explanation { get; set; }
    public int TimeLimitSeconds { get; set; } = 60; // 5..3600
    public bool IsActive { get; set; } = true;
}
