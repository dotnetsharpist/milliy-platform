namespace MilliyMock.Service.Dtos.Practice;

public class PracticeTopicAdminResultDto
{
    public long Id { get; set; }
    public int Subject { get; set; }
    public string Slug { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Section { get; set; }
    public int Order { get; set; }
    public int QuestionCount { get; set; }
}
