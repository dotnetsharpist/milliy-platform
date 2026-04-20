using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class QuestionGroupAttemptDto
{
    public long Id { get; set; }
    public List<TranslationResultDto> Translations { get; set; } = [];
    public List<QuestionAttemptForQuestionGroupDto> Questions { get; set; } = [];
    public List<OptionAttemptDto> Options { get; set; } = [];
}