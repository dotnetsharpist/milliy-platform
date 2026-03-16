using MilliyMock.Models;
using MilliyMock.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MilliyMock.Controllers;

[Route("api/user")]
public class UserController(IUserService userService) : BaseController
{
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,User")]
    public async Task<IActionResult> GetMe()
        => Ok(new Response
        {
            Data = await userService.GetMe()
        });
}
