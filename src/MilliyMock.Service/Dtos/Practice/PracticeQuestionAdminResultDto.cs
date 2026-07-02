namespace MilliyMock.Service.Dtos.Practice;

// Admin view — includes the correct answer and active flag,
// unlike the student-facing PracticeQuestionResultDto.
public class PracticeQuestionAdminResultDto
{
    public long Id { get; set; }
    public int Subject { get; set; }
    public int? Grade { get; set; }
    public int Difficulty { get; set; }
    public string Topic { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string OptionA { get; set; } = null!;
    public string OptionB { get; set; } = null!;
    public string OptionC { get; set; } = null!;
    public string OptionD { get; set; } = null!;
    public string CorrectLetter { get; set; } = null!;
    public string? ExplanationTitle { get; set; }
    public string? Explanation { get; set; }
    public int TimeLimitSeconds { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
