namespace MilliyMock.Service.Dtos.UserBalances;

public class AdjustBalanceDto
{
    public long UserId { get; set; }
    public int Amount { get; set; }
    public string? Description { get; set; }
}
