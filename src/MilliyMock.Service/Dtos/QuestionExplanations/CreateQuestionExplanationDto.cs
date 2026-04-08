using Microsoft.AspNetCore.Http;

namespace MilliyMock.Service.Dtos.QuestionExplanations;

public class CreateQuestionExplanationDto
{
    public long QuestionId { get; set; }
    public string Text { get; set; } = null!;
    public IFormFile? Image { get; set; }
}