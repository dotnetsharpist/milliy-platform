using MilliyMock.Service.Dtos.QuestionGroups;

namespace MilliyMock.Service.Interfaces;

public interface IQuestionGroupService
{
    Task<bool> CreateAsync(CreateQuestionGroupDto dto);
    Task<List<QuestionGroupResultDto>> GetByTestIdAsync(long testId);
    Task<bool> DeleteAsync(long questionGroupId);
}