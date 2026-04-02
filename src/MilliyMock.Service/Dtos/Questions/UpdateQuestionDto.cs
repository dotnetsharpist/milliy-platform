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

    /// <summary>
    /// Full list of options after update.
    /// Options with Id > 0 will be updated; Id == 0 will be created; any existing option
    /// whose Id is not present in this list will be soft-deleted.
    /// Omit (null) to leave options unchanged.
    /// </summary>
    public List<UpdateQuestionOptionDto>? Options { get; set; }
}