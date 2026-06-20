using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.UserTestAttempt;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/user-test-attempt")]
public class UserTestAttemptController(IUserTestAttemptService service) : BaseController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(CreateUserTestAttemptDto dto)
        => Ok(new Response
        {
            Data = await service.CreateAsync(dto)
        });
    
    [HttpGet("get-user-attempts")]
    [Authorize]
    public async Task<IActionResult> GetByUserId()
        => Ok(new Response
        {
            Data = await service.GetByUserId()
        });

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetById([FromQuery] long testAttemptId)
        => Ok(new Response
        {
            Data = await service.GetById(testAttemptId)
        });
    
    [HttpGet("get-by-test-id")]
    [Authorize]
    public async Task<IActionResult> GetByTestId([FromQuery] long testId)
        => Ok(new Response
        {
            Data = await service.GetByTestId(testId)
        });


    [HttpPost("submit")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitAsync(long testAttemptId)
        => Ok(new Response
        {
            Data = await service.SubmitTest(testAttemptId)
        });
    
    [HttpGet("get-progress")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProgress(long testId)
        => Ok(new Response
        {
            Data = await service.GetProgressAsync(testId)
        });
}