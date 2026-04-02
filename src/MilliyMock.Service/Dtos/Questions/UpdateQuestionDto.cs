using Microsoft.AspNetCore.Http;
using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Options;

namespace MilliyMock.Service.Dtos.Questions;

public class UpdateQuestionDto
{
    public string? Text { get; set; }
    public IFormFile? Image { get; set; }
    public int Order { get; set; }
    public decimal Score { get; set; }
    public QuestionType Type { get; set; }
    public string? CorrectAnswer { get; set; }
}