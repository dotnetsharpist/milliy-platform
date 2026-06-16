using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

public class TransactionHistory : Auditable
{
    public long UserId { get; set; }
    public User User { get; set; }
    public int Amount { get; set; }
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public BalanceTransactionType Type { get; set; }
    public string? Description { get; set; }

    /*
    public long? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    */
    // soon lol

    public long? TestId { get; set; }
    public Test? Test { get; set; }
}