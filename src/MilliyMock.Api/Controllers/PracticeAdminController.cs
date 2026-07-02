using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Domain.Enums;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.Practice;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

// Content management for the practice (mashq) bank. Usable directly from
// Swagger until a dedicated admin UI exists.
[Route("api/practice/admin")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class PracticeAdminController(IPracticeAdminService adminService) : BaseController
{
    // ---- Topics ----

    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics([FromQuery] SubjectType? subject)
        => Ok(new Response { Data = await adminService.GetTopicsAsync(subject) });

    [HttpPost("topics")]
    public async Task<IActionResult> CreateTopic(SavePracticeTopicDto dto)
        => Ok(new Response { Data = await adminService.CreateTopicAsync(dto) });

    [HttpPut("topics/{id:long}")]
    public async Task<IActionResult> UpdateTopic(long id, SavePracticeTopicDto dto)
        => Ok(new Response { Data = await adminService.UpdateTopicAsync(id, dto) });

    [HttpDelete("topics/{id:long}")]
    public async Task<IActionResult> DeleteTopic(long id)
        => Ok(new Response { Data = await adminService.DeleteTopicAsync(id) });

    // ---- Questions ----

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] SubjectType? subject,
        [FromQuery] string? topic,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(new Response { Data = await adminService.GetQuestionsAsync(subject, topic, page, pageSize) });

    [HttpPost("questions")]
    public async Task<IActionResult> CreateQuestion(SavePracticeQuestionDto dto)
        => Ok(new Response { Data = await adminService.CreateQuestionAsync(dto) });

    [HttpPut("questions/{id:long}")]
    public async Task<IActionResult> UpdateQuestion(long id, SavePracticeQuestionDto dto)
        => Ok(new Response { Data = await adminService.UpdateQuestionAsync(id, dto) });

    [HttpDelete("questions/{id:long}")]
    public async Task<IActionResult> DeleteQuestion(long id)
        => Ok(new Response { Data = await adminService.DeleteQuestionAsync(id) });
}
