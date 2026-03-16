using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Interfaces;

public interface IUserService
{
    ValueTask<bool> Add(UserCreationDto dto);
    ValueTask<bool> Update(long id, UserUpdateDto dto);
    ValueTask<bool> Delete(long id);
    ValueTask<List<UserResultDto>> GetAll();
    ValueTask<UserResultDto> GeById(long id);
    ValueTask<UserResultDto> GetMe();
}