using MilliyMock.Service.Dtos.Tests;

namespace MilliyMock.Service.Interfaces;

public interface ITestService
{
    Task<bool> CreateAsync(CreateTestDto dto);
    Task<bool> UpdateAsync(long testId, UpdateTestDto dto);
    Task<bool> DeleteAsync(long testId);
    Task<List<TestResultDto>> GetAllAsync();
    Task<FullTestResultDto> GetFullTest(long testId);
}