using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/test")]
public class TestController(ITestService testService) : BaseController
{
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateAsync(CreateTestDto dto)
        => Ok(new Response
        {
            Data = await testService.CreateAsync(dto)
        });

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
        => Ok(new Response
        {
            Data = await testService.GetAllAsync()
        });
}