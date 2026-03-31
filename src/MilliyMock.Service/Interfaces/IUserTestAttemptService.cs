using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserTestAttempt;

namespace MilliyMock.Service.Interfaces;

public interface IUserTestAttemptService
{
    Task<UserTestAttemptResultDto> CreateAsync(CreateUserTestAttemptDto dto);
    Task<AttemptedTestResultDto> SubmitTest(long testAttemptId);
    Task<bool> PauseTest(long testId);
    Task<List<UserTestAttemptResultDto>> GetByUserId();
    Task<List<UserTestAttemptResultDto>> GetProgressAsync(long testId);
}