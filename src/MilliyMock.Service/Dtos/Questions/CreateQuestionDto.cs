using Microsoft.AspNetCore.Http;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Questions;

public class CreateQuestionDto
{
    public string? Text { get; set; }
    public IFormFile? Image { get; set; }
    public int Order { get; set; }
    public int Score { get; set; }
    public QuestionType Type { get; set; }
    public long TestId { get; set; }
    public string? CorrectAnswer { get; set; }
    public long? QuestionGroupId { get; set; }
}