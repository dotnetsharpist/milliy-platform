using Microsoft.AspNetCore.Http;
using MilliyMock.Domain.Enums;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Dtos.QuestionExplanations;

namespace MilliyMock.Service.Dtos.Questions;

public class UpdateQuestionDto
{
    public string? TextUz { get; set; }
    public string? TextRu { get; set; }
    public IFormFile? ImageUz { get; set; }
    public IFormFile? ImageRu { get; set; }
    public int Order { get; set; }
    public decimal Score { get; set; }
    public QuestionType Type { get; set; }
    public string? CorrectAnswer { get; set; }
    public List<UpdateQuestionOptionDto>? Options { get; set; }
    public UpdateQuestionExplanationDto? Explanation { get; set; }
}