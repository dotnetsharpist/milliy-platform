using MilliyMock.Service.Dtos.Options;

namespace MilliyMock.Service.Interfaces;

public interface IOptionService
{
    Task<bool> CreateAsync(CreateOptionDto dto);
    Task<bool> DeleteAsync(long optionId);
}