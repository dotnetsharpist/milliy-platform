using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Service.Interfaces;

public interface IUserService
{
    ValueTask<bool> Add(CreateUserDto dto);
    ValueTask<bool> Update(UpdateUserDto dto);
    ValueTask<bool> Delete(long id);
    ValueTask<List<UserResultDto>> GetAll();
    ValueTask<UserResultDto> GeById(long id);
    ValueTask<UserResultDto?> GeByTelegramUserId(long id);
    ValueTask<UserResultDto> GetMe();
}