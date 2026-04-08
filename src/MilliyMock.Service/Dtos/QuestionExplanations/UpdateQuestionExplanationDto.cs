using Microsoft.AspNetCore.Http;

namespace MilliyMock.Service.Dtos.QuestionExplanations;

public class UpdateQuestionExplanationDto
{
    public string Text { get; set; } = null!;
    public IFormFile? Image { get; set; }
}