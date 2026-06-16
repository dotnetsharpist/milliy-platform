using MilliyMock.Service.Dtos.BotUsers;

namespace MilliyMock.Service.Interfaces;

public interface IBotUserService
{
    ValueTask<bool> CreateAsync(CreateBotUserDto dto);
    ValueTask<List<BotUserResultDto>> GetAllAsync();
    ValueTask<BotUserResultDto> GeByIdAsync(long id);
    Task<string> AddBalanceViaBotAsync(string messageText);
}