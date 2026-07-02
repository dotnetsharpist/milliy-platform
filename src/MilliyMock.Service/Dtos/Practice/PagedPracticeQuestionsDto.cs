namespace MilliyMock.Service.Dtos.Practice;

public class PagedPracticeQuestionsDto
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<PracticeQuestionAdminResultDto> Items { get; set; } = [];
}
