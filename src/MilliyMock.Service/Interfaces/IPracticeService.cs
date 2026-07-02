using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Practice;

namespace MilliyMock.Service.Interfaces;

public interface IPracticeService
{
    Task<List<PracticeQuestionResultDto>> GetQuestionsAsync(
        SubjectType subject, int? grade, PracticeDifficulty? difficulty, string? topic, string? status, int count);

    Task<PracticeAnswerResultDto> AnswerAsync(PracticeAnswerDto dto);
    Task<PracticeQuotaResultDto> GetQuotaAsync();
    Task<PracticeQuotaResultDto> PurchaseQuotaAsync();
    Task<PracticeSaveResultDto> ToggleSaveAsync(long questionId);
    Task<List<PracticeTopicResultDto>> GetTopicsAsync(SubjectType subject);
}
