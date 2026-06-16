using MilliyMock.Service.Dtos.TransactionHistories;

namespace MilliyMock.Service.Interfaces;

public interface ITransactionHistoryService
{
    Task<List<TransactionHistoryResultDto>> GetMyTransactionsAsync();
    Task<List<TransactionHistoryResultDto>> GetByUserIdAsync(long userId);
}
