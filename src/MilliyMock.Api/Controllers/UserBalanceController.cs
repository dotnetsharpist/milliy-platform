using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.UserBalances;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/balance")]
public class UserBalanceController(IUserBalanceService balanceService) : BaseController
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyBalance()
        => Ok(new Response
        {
            Data = await balanceService.GetMyBalanceAsync()
        });

    [HttpGet("{userId:long}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetByUserId(long userId)
        => Ok(new Response
        {
            Data = await balanceService.GetByUserIdAsync(userId)
        });

    /*
    [HttpPost("deposit")]
    [Authorize]
    public async Task<IActionResult> Deposit(DepositDto dto)
        => Ok(new Response
        {
            Data = await balanceService.DepositAsync(dto)
        });
        */

    [HttpPost("adjust")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Adjust(AdjustBalanceDto dto)
        => Ok(new Response
        {
            Data = await balanceService.AdjustAsync(dto)
        });
}