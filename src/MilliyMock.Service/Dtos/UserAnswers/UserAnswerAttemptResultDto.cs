namespace MilliyMock.Service.Dtos.UserAnswers;

public class UserAnswerAttemptResultDto
{
    public long QuestionId { get; set; }
    public long? SelectedOptionId { get; set; }
    public string? TextAnswer { get; set; }

    // Server-graded correctness for this answer. The client renders the review
    // badge straight from this — it does not re-grade — so the badge, the counts
    // and the score all come from one grading pass.
    public bool IsCorrect { get; set; }
}
