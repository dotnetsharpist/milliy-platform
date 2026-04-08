using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class QuestionGroupResultDto
{
    public long Id { get; set; }
    public long TestId { get; set; }
    public int QuestionCount { get; set; }
    public List<TranslationResultDto> Translations { get; set; } = [];
    public List<QuestionResultDto> Questions { get; set; } = [];
    public List<OptionResultDto> Options { get; set; } = [];
}