using Microsoft.AspNetCore.Http;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class CreateQuestionGroupDto
{
    public string? Title { get; set; }
    public IFormFile? Image { get; set; }
    public long TestId { get; set; }
}