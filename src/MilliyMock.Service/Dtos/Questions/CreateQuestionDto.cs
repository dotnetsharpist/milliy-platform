using Microsoft.AspNetCore.Http;
using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Options;

namespace MilliyMock.Service.Dtos.Questions;

public class CreateQuestionDto
{
    public string? TextUz { get; set; }
    public string? TextRu { get; set; }
    public IFormFile? ImageUz { get; set; }
    public IFormFile? ImageRu { get; set; }
    public int Order { get; set; }
    public decimal Score { get; set; }
    public QuestionType Type { get; set; }
    public long? TestId { get; set; }
    public string? CorrectAnswer { get; set; }
    public long? QuestionGroupId { get; set; }
    public List<CreateQuestionOptionDto>? Options { get; set; }
}