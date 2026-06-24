using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class QuestionGroupAttemptResultDto
{
    public long Id { get; set; }
    public List<TranslationResultDto> Translations { get; set; } = [];
    // Results-only sub-question shape that includes the correct answer (see
    // QuestionResultForGroupDto). The exam variant (QuestionGroupAttemptDto)
    // still uses the answer-less QuestionAttemptForQuestionGroupDto.
    public List<QuestionResultForGroupDto> Questions { get; set; } = [];
    public List<OptionAttemptResultDto> Options { get; set; } = [];
}
