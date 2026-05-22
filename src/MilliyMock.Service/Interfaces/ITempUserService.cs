using MilliyMock.Service.Dtos.TempUsers;

namespace MilliyMock.Service.Interfaces;

public interface ITempUserService
{
    Task<TempUserResultDto> CreateAsync(CreateTempUserDto dto);
}