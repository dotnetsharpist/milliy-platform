using MilliyMock.Models;
using MilliyMock.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Service.Dtos.Users;

namespace MilliyMock.Controllers;

[Route("api/user")]
public class UserController(IUserService userService) : BaseController
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMe()
        => Ok(new Response
        {
            Data = await userService.GetMe()
        });

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update(UpdateUserDto dto)
        => Ok(new Response
        {
            Data = await userService.Update(dto)
        });
}
