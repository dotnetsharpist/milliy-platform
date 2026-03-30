using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserTestAttempt;

namespace MilliyMock.Service.Interfaces;

public interface IUserTestAttemptService
{
    Task<UserTestAttemptResultDto> CreateAsync(CreateUserTestAttemptDto dto);
    Task<AttemptedTestResultDto> SubmitTest();
    Task<List<UserTestAttemptResultDto>> GetByUserId();
}