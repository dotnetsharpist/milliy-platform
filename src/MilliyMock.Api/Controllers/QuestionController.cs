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
    [RequestSizeLimit(104857600)]
    public async Task<IActionResult> CreateAsync([FromForm] CreateQuestionDto dto)
        => Ok(new Response
        {
            Data = await questionService.CreateAsync(dto)
        });
    
    [HttpPost("bulk")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateManyAsync([FromForm] CreateManyQuestionsDto dto)
        => Ok(new Response
        {
            Data = await questionService.CreateManyAsync(dto)
        });

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetByTestIdAsync([FromQuery] long testId)
        => Ok(new Response
        {
            Data = await questionService.GetByTestIdAsync(testId)
        });
    
    [HttpGet("{questionId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetByIdAsync(long questionId)
        => Ok(new Response
        {
            Data = await questionService.GetByIdAsync(questionId)
        });

    
    [HttpDelete("{questionId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAsync(long questionId)
        => Ok(new Response
        {
            Data = await questionService.DeleteAsync(questionId)
        });

    [HttpPut("{questionId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateAsync(long questionId, [FromForm] UpdateQuestionDto dto)
        => Ok(new Response
        {
            Data = await questionService.UpdateAsync(questionId, dto)
        });
}