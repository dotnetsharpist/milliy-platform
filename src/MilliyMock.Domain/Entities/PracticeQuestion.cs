using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class PracticeQuestion : Auditable
{
    public SubjectType Subject { get; set; }
    public int? Grade { get; set; } // 5..11, null = any grade
    public PracticeDifficulty Difficulty { get; set; }
    public string Topic { get; set; } = null!; // slug: "algebra", "geometriya", ...
    public string Text { get; set; } = null!; // may contain LaTeX: $...$ and $$...$$
    public string OptionA { get; set; } = null!;
    public string OptionB { get; set; } = null!;
    public string OptionC { get; set; } = null!;
    public string OptionD { get; set; } = null!;
    public string CorrectLetter { get; set; } = null!; // "A".."D" — never serialize to client before answer
    public string? ExplanationTitle { get; set; }
    public string? Explanation { get; set; } // paragraphs separated by \n\n, LaTeX allowed
    public bool IsActive { get; set; } = true;

    public ICollection<PracticeAttempt> Attempts { get; set; } = new List<PracticeAttempt>();
}
