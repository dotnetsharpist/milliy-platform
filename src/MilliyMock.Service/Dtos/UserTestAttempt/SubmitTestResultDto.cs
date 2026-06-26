using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.UserTestAttempt;

// Submit is a pure state transition now — it just confirms the attempt was
// finished. The graded payload (score, counts, answers) is read from
// get-results, so this stays intentionally small.
public class SubmitTestResultDto
{
    public long TestAttemptId { get; set; }
    public AttemptStatus AttemptStatus { get; set; }
}
