using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.TempUsers;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/temp-user")]
public class TempUserController(ITempUserService service) : BaseController
{
    public async Task<IActionResult> CreateAsync(CreateTempUserDto dto)
        => Ok(new Response
        {
            Data = await service.CreateAsync(dto)
        });
    
}