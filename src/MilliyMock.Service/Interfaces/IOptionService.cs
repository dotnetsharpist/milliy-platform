using MilliyMock.Service.Dtos.Options;

namespace MilliyMock.Service.Interfaces;

public interface IOptionService
{
    Task<bool> CreateAsync(CreateOptionDto dto);
    Task<List<OptionResultDto>> GetByQuestionIdAsync(long? questionId, long? questionGroupId);
    Task<bool> UpdateAsync(long optionId, UpdateOptionDto dto);
    Task<bool> DeleteAsync(long optionId);
}