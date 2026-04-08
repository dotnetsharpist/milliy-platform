using Microsoft.AspNetCore.Http;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class UpdateQuestionGroupDto
{
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public IFormFile? ImageUz { get; set; }
    public IFormFile? ImageRu { get; set; }
}

