using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Interfaces;

public interface IQuestionService
{
    Task<bool> CreateAsync(CreateQuestionDto dto);
    Task<List<QuestionResultDto>> GetByTestIdAsync(long testId);
    Task<bool> DeleteAsync(long questionId);
}