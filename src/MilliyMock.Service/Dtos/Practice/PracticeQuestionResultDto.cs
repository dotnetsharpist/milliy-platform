namespace MilliyMock.Service.Dtos.Practice;

// Question as served to the student — NEVER include the correct letter or
// explanation here; those are only revealed by the answer endpoint.
public class PracticeQuestionResultDto
{
    public long Id { get; set; }
    public int Subject { get; set; } // SubjectType as int — frontend maps it
    public int? Grade { get; set; }
    public int Difficulty { get; set; } // PracticeDifficulty as int
    public string Topic { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string OptionA { get; set; } = null!;
    public string OptionB { get; set; } = null!;
    public string OptionC { get; set; } = null!;
    public string OptionD { get; set; } = null!;
    public int TimeLimitSeconds { get; set; }
    public bool IsSaved { get; set; }
    public string? LastResult { get; set; } // "correct" | "incorrect" | null
}
