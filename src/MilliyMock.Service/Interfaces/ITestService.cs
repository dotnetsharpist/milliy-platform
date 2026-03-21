using MilliyMock.Service.Dtos.Tests;

namespace MilliyMock.Service.Interfaces;

public interface ITestService
{
    Task<bool> CreateAsync(CreateTestDto dto);
    Task<List<TestResultDto>> GetAllAsync();
}