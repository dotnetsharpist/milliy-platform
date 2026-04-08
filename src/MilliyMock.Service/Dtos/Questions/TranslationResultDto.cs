using MilliyMock.Domain.Enums;

namespace MilliyMock.Service.Dtos.Questions;

public class TranslationResultDto
{
    public long Id { get; set; }
    public Language Language { get; set; }
    public string Text { get; set; } = null!;
    public string? ImagePath { get; set; }
}

