using MilliyMock.Service.Dtos.QuestionExplanations;

namespace MilliyMock.Service.Interfaces;

public interface IQuestionExplanationService
{
    Task<bool> CreateAsync(CreateQuestionExplanationDto dto);
    Task<bool> UpdateAsync(long questionExplanationId, UpdateQuestionExplanationDto dto);
    Task<QuestionExplanationResultDto> GetByQuestionIdAsync(long questionId);
    Task<bool> DeleteAsync(long questionExplanationId);
}