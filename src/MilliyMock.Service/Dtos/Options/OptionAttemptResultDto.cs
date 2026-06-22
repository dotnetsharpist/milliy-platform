namespace MilliyMock.Service.Dtos.Options;

public class OptionAttemptResultDto
{
    public long Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}
