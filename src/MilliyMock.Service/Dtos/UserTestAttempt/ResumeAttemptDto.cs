using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.UserAnswers;

namespace MilliyMock.Service.Dtos.UserTestAttempt;

// Resume payload for an in-progress attempt, keyed by attempt id. Carries the
// same answer-less exam content as start-test (so a refreshed page can render
// without minting a new attempt) plus the answers already saved for this attempt
// and the server-computed time left. If the attempt's window has elapsed it is
// finished server-side and returned with IsCompleted = true so the client routes
// to the results screen instead of resuming.
public class ResumeAttemptDto
{
    public long TestAttemptId { get; set; }
    public long TestId { get; set; }
    public string Title { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public int RemainingSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public List<QuestionAttemptDto> Questions { get; set; } = [];
    public List<QuestionGroupAttemptDto> QuestionGroups { get; set; } = [];
    public List<UserAnswerAttemptResultDto> UserAnswers { get; set; } = [];
}
