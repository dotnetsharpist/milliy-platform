namespace MilliyMock.Service.Dtos.Tests;

public class AttemptedTestResultDto
{
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
}