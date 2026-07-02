using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Domain.Enums;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.Practice;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/practice")]
[Authorize]
public class PracticeController(IPracticeService practiceService) : BaseController
{
    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] SubjectType subject,
        [FromQuery] int? grade,
        [FromQuery] PracticeDifficulty? difficulty,
        [FromQuery] string? topic,
        [FromQuery] string? status,
        [FromQuery] int count = 12)
        => Ok(new Response
        {
            Data = await practiceService.GetQuestionsAsync(subject, grade, difficulty, topic, status, count)
        });

    [HttpPost("answer")]
    public async Task<IActionResult> Answer(PracticeAnswerDto dto)
        => Ok(new Response
        {
            Data = await practiceService.AnswerAsync(dto)
        });

    [HttpGet("quota")]
    public async Task<IActionResult> GetQuota()
        => Ok(new Response
        {
            Data = await practiceService.GetQuotaAsync()
        });

    [HttpPost("quota/purchase")]
    public async Task<IActionResult> PurchaseQuota()
        => Ok(new Response
        {
            Data = await practiceService.PurchaseQuotaAsync()
        });

    [HttpPost("save")]
    public async Task<IActionResult> ToggleSave(PracticeAnswerDto dto)
        => Ok(new Response
        {
            Data = await practiceService.ToggleSaveAsync(dto.QuestionId)
        });

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics([FromQuery] SubjectType subject)
        => Ok(new Response
        {
            Data = await practiceService.GetTopicsAsync(subject)
        });
}
