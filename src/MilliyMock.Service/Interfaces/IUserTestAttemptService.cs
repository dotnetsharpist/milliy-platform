using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserTestAttempt;

namespace MilliyMock.Service.Interfaces;

public interface IUserTestAttemptService
{
    Task<StartTestResultDto> StartTestAsync(CreateUserTestAttemptDto dto);
    Task<TestAttemptResultsDto> GetResultsAsync(long testAttemptId);
    Task<AttemptedTestResultDto> SubmitTest(long testAttemptId);
    Task<List<UserTestAttemptResultDto>> GetByUserId();
    Task<UserTestAttemptResultDto> GetById(long testAttemptId);
    Task<UserTestAttemptResultDto?> GetByTestId(long testId);
    Task<UserTestAttemptResultDto> GetProgressAsync(long testId);
}