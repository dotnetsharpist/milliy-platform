using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/user-answer")]
public class UserAnswerController(IUserAnswerService answerService) : BaseController
{
    // Idempotent upsert: call on every answer change. Both verbs are kept so existing
    // POST (create) and PUT (update) clients both work; consolidate to one on the frontend.
    [HttpPost]
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> SetAsync(CreateUserAnswerDto dto)
        => Ok(new Response
        {
            Data = await answerService.SetAnswerAsync(dto)
        });

}

