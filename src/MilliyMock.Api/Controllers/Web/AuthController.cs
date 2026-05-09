using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.Auth;
using MilliyMock.Service.Dtos.Users;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers.Web;

[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseController
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromForm] LoginDto dto)
        => Ok(new Response
        {
            Code = 200,
            Message = "Ok👍🏿",
            Data = await authService.Login(dto)
        });

    [HttpPost("telegram-login")]
    [AllowAnonymous]
    public async Task<IActionResult> TelegramLogin(TelegramLoginDto dto)
        => Ok(new Response
        {
            Code = 200,
            Message = "Ok👍🏿",
            Data = await authService.TelegramLogin(dto)
        });
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync(CreateUserDto dto)
        => Ok(new Response
        {
            Code = 200,
            Message = "Ok👍🏿",
            Data = await authService.Register(dto)
        });
}