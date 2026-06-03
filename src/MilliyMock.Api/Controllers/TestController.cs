using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Domain.Enums;
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
    
    [HttpPut("{testId:long}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateTest(long testId, [FromBody] UpdateTestDto dto)
        => Ok(new Response
        {
            Data = await testService.UpdateAsync(testId, dto)
        });
    
    [HttpDelete("{testId:long}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAsync(long testId)
        => Ok(new Response
        {
            Data = await testService.DeleteAsync(testId)
        });


    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SubjectType? subject)
        => Ok(new Response
        {
            Data = await testService.GetAllAsync(subject)
        });
    
    /*
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetByIdAsync([FromQuery] long testId)
        => Ok(new Response
        {
            Data = await testService.GetByIdAsync(testId)
        });
        */
    
    [AllowAnonymous]
    [HttpGet("{testId:long}")]
    public async Task<IActionResult> GetTheWholeTest(long testId)
        => Ok(new Response
        {
            Data = await testService.GetFullTest(testId)
        });
}