using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.QuestionExplanations;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/question-explanation")]
public class QuestionExplanationController(IQuestionExplanationService questionExplanationService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetByQuestionIdAsync([FromQuery] long questionId)
        => Ok(new Response
        {
            Data = await questionExplanationService.GetByQuestionIdAsync(questionId)
        });
    
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateQuestionExplanationDto dto)
        => Ok(new Response
        {
            Data = await questionExplanationService.CreateAsync(dto)
        });
    
    [HttpPut("{questionExplanationId}")]
    public async Task<IActionResult> UpdateAsync(long questionExplanationId, UpdateQuestionExplanationDto dto)
        => Ok(new Response
        {
            Data = await questionExplanationService.UpdateAsync(questionExplanationId, dto) 
        });
    
    [HttpDelete("{questionExplanationId}")]
    public async Task<IActionResult> DeleteAsync(long questionExplanationId)
        => Ok(new Response
        {
            Data = await questionExplanationService.DeleteAsync(questionExplanationId)
        });
}