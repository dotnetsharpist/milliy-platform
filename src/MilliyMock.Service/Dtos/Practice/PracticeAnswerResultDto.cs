namespace MilliyMock.Service.Dtos.Practice;

public class PracticeAnswerResultDto
{
    public bool IsCorrect { get; set; }
    public string CorrectLetter { get; set; } = null!;
    public string? ExplanationTitle { get; set; }
    public string? Explanation { get; set; }
    public int QuotaRemaining { get; set; }
}
