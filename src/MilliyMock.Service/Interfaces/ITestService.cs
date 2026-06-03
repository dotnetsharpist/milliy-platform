using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Tests;

namespace MilliyMock.Service.Interfaces;

public interface ITestService
{
    Task<bool> CreateAsync(CreateTestDto dto);
    Task<bool> UpdateAsync(long testId, UpdateTestDto dto);
    Task<bool> DeleteAsync(long testId);
    Task<List<TestResultDto>> GetAllAsync(SubjectType? subject);
    Task<TestResultDto> GetByIdAsync(long testId);
    Task<FullTestResultDto> GetFullTest(long testId);
}