using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/question")]
public class QuestionController(IQuestionService questionService) : BaseController
{
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateAsync([FromForm] CreateQuestionDto dto)
        => Ok(new Response
        {
            Data = await questionService.CreateAsync(dto)
        });

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetByTestIdAsync(long testId)
        => Ok(new Response
        {
            Data = await questionService.GetByTestIdAsync(testId)
        });
    
    [HttpDelete("{questionId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAsync(long questionId)
        => Ok(new Response
        {
            Data = await questionService.DeleteAsync(questionId)
        });
}