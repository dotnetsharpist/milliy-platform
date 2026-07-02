namespace MilliyMock.Service.Dtos.Practice;

public class PracticeAnswerDto
{
    public long QuestionId { get; set; }
    public string Letter { get; set; } = null!; // "A".."D"
}
