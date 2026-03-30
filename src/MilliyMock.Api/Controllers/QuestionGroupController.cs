using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/question-group")]
public class QuestionGroupController(IQuestionGroupService groupService) : BaseController
{
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateAsync(CreateQuestionGroupDto dto)
        => Ok(new Response
        {
            Data = await groupService.CreateAsync(dto)
        });
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetByTestIdAsync(long testId)
        => Ok(new Response
        {
            Data = await groupService.GetByTestIdAsync(testId)
        });
    
    [HttpDelete("{questionGroupId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAsync(long questionGroupId)
        => Ok(new Response
        {
            Data = await groupService.DeleteAsync(questionGroupId)
        });
}