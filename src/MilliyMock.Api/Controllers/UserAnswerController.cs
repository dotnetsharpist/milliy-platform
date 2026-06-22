using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MilliyMock.Models;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Controllers;

[Route("api/user-answer")]
public class UserAnswerController(IUserAnswerService answerService) : BaseController
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(CreateUserAnswerDto dto)
        => Ok(new Response
        {
            Data = await answerService.CreateAsync(dto)
        });
    
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateAsync(UpdateUserAnswerDto dto)
        => Ok(new Response
        {
            Data = await answerService.UpdateAsync(dto)
        });

}

