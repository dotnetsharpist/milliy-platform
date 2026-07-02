using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Practice;

namespace MilliyMock.Service.Interfaces;

public interface IPracticeAdminService
{
    // Topics
    Task<List<PracticeTopicAdminResultDto>> GetTopicsAsync(SubjectType? subject);
    Task<PracticeTopicAdminResultDto> CreateTopicAsync(SavePracticeTopicDto dto);
    Task<PracticeTopicAdminResultDto> UpdateTopicAsync(long id, SavePracticeTopicDto dto);
    Task<bool> DeleteTopicAsync(long id);

    // Questions
    Task<PagedPracticeQuestionsDto> GetQuestionsAsync(SubjectType? subject, string? topic, int page, int pageSize);
    Task<PracticeQuestionAdminResultDto> CreateQuestionAsync(SavePracticeQuestionDto dto);
    Task<PracticeQuestionAdminResultDto> UpdateQuestionAsync(long id, SavePracticeQuestionDto dto);
    Task<bool> DeleteQuestionAsync(long id);
}
