namespace MilliyMock.Shared.Helpers;

public static class TimeHelper
{
    private const int UTC = 5;
    public static DateTime GetDateTime()
    {
        var dtTime = DateTime.UtcNow;
        dtTime = dtTime.AddHours(UTC);
        return dtTime;
    }
    
    public static DateOnly GetDateOnly()
    {
        return DateOnly.FromDateTime(GetDateTime());
    }
    
    public static TimeOnly GetTimeOnly()
    {
        return TimeOnly.FromDateTime(GetDateTime());
    }
}