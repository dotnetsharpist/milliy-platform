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
    
    [HttpPost("google-login")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
        => Ok(new Response
        {
            Code = 200,
            Message = "Ok👍🏿",
            Data = await authService.GoogleLogin(dto)
        });

    /*
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync(RegisterDto dto)
        => Ok(new Response
        {
            Code = 200,
            Message = "Ok👍🏿",
            Data = await authService.Register(dto)
        });
        */

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmailAsync(VerifyEmailDto dto)
        => Ok(new Response
        {
            Code = 200,
            Message = "Ok👍🏿",
            Data = await authService.VerifyEmail(dto)
        });

    [HttpPost("resend-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendOtpAsync(ResendOtpDto dto)
        => Ok(new Response
        {
            Code = 200,
            Message = "Ok👍🏿",
            Data = await authService.ResendOtp(dto)
        });
}