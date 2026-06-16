using MilliyMock.Service.Dtos.UserBalances;

namespace MilliyMock.Service.Interfaces;

public interface IUserBalanceService
{
    Task<UserBalanceResultDto> GetMyBalanceAsync();
    Task<UserBalanceResultDto> GetByUserIdAsync(long userId);
    Task<bool> AdjustAsync(AdjustBalanceDto dto);
    Task<bool> PurchaseTestAsync(long testId);
}
