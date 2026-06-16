using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.TransactionHistories;

public class TransactionHistoryResultDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int Amount { get; set; }
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public BalanceTransactionType Type { get; set; }
    public string? Description { get; set; }
    public long? TestId { get; set; }
    public DateTime CreatedAt { get; set; }
}
