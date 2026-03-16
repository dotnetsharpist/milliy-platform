namespace MilliyMock.Domain.Configurations;

public class PaginationParams
{
    private const int MaxPageSize = 20;
    private int _pageSize;
    public int PageSize
    {
        get => _pageSize == 0 ? 20 : _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    public int PageIndex { get; set; } = 1;
}
