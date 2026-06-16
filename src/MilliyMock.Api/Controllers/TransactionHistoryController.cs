using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/transaction")]
public class TransactionHistoryController(ITransactionHistoryService transactionService) : BaseController
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyTransactions()
        => Ok(new Response
        {
            Data = await transactionService.GetMyTransactionsAsync()
        });

    [HttpGet("{userId:long}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetByUserId(long userId)
        => Ok(new Response
        {
            Data = await transactionService.GetByUserIdAsync(userId)
        });
}
