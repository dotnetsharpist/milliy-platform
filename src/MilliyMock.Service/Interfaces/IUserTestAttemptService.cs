using MilliyMock.Service.Dtos.UserTestAttempt;

namespace MilliyMock.Service.Interfaces;

public interface IUserTestAttemptService
{
    Task<bool> CreateAsync(CreateUserTestAttemptDto dto);
    Task<List<UserTestAttemptResultDto>> GetByUserId();
}