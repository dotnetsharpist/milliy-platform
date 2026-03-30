using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/option")]
public class OptionController(IOptionService optionService) : BaseController
{
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateAsync(CreateOptionDto dto)
        => Ok(new Response
        {
            Data = await optionService.CreateAsync(dto)
        });
    
    [HttpDelete("{optionId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAsync(long optionId)
        => Ok(new Response
        {
            Data = await optionService.DeleteAsync(optionId)
        });
}