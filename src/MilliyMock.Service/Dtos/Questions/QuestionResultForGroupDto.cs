using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Options;

namespace MilliyMock.Service.Dtos.Questions;

// Results-only shape for a sub-question inside a question group. Unlike the
// exam DTO (QuestionAttemptForQuestionGroupDto), this one carries the answer
// information — CorrectAnswer (FreeAnswer) and Options with IsCorrect
// (MultipleChoice) — so the review screen can show the correct answer once the
// attempt is completed. It is referenced only from QuestionGroupAttemptResultDto
// (results), so it never leaks answers into the live exam payload.
public class QuestionResultForGroupDto
{
    public long Id { get; set; }
    public int Order { get; set; }
    public decimal Score { get; set; }
    public QuestionType Type { get; set; }
    public long? QuestionGroupId { get; set; }
    public string? CorrectAnswer { get; set; }
    public List<TranslationResultDto> Translations { get; set; } = [];
    public List<OptionAttemptResultDto> Options { get; set; } = [];
}
