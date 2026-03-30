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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateAsync(CreateUserTestAttemptDto dto)
        => Ok(new Response
        {
            Data = await service.CreateAsync(dto)
        });
    
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CreateAsync()
        => Ok(new Response
        {
            Data = await service.GetByUserId()
        });

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> SubmitAsync()
        => Ok(new Response
        {
            Data = await service.SubmitTest()
        });
}