namespace MilliyMock.Service.Dtos.Practice;

public class PracticeQuotaResultDto
{
    public int FreeLimit { get; set; }
    public int Used { get; set; }
    public int Extra { get; set; }
    public int Remaining { get; set; }
}
