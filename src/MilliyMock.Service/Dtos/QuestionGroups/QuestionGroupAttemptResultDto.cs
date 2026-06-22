using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class QuestionGroupAttemptResultDto
{
    public long Id { get; set; }
    public List<TranslationResultDto> Translations { get; set; } = [];
    public List<QuestionAttemptForQuestionGroupDto> Questions { get; set; } = [];
    public List<OptionAttemptResultDto> Options { get; set; } = [];
}
