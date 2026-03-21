using MilliyMock.Service.Dtos.UserAnswers;

namespace MilliyMock.Service.Interfaces;

public interface IUserAnswerService
{
    Task<bool> CreateAsync(CreateUserAnswerDto dto);
}