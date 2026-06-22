using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Interfaces;

public interface ITransactionService
{
    Task<bool> ApplyTransactionAsync(long userId, int amount, BalanceTransactionType type, string? description, long? testId);
}