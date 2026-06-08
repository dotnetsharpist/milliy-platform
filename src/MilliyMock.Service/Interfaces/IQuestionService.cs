using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Interfaces;

public interface IQuestionService
{
    Task<bool> CreateAsync(CreateQuestionDto dto);
    Task<CreateManyQuestionsResultDto> CreateManyAsync(CreateManyQuestionsDto dto);
    Task<bool> UpdateAsync(long questionId, UpdateQuestionDto dto);
    Task<List<QuestionResultDto>> GetByTestIdAsync(long testId);
    Task<QuestionResultDto> GetByIdAsync(long questionId);
    Task<bool> DeleteAsync(long questionId);
}