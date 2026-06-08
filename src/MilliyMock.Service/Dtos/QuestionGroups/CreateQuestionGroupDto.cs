using Microsoft.AspNetCore.Http;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.QuestionExplanations;
using MilliyMock.Service.Dtos.Questions;

namespace MilliyMock.Service.Dtos.QuestionGroups;

public class CreateQuestionGroupDto
{
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public IFormFile? ImageUz { get; set; }
    public IFormFile? ImageRu { get; set; }
    public long TestId { get; set; }
    
    public CreateQuestionExplanationDto? Explanation { get; set; }
    public List<CreateQuestionDto>? Questions { get; set; }
    public List<CreateOptionDto>? Options { get; set; }
}