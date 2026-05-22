using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class UserAnswerService(IUnitOfWork unitOfWork, IMapper mapper) : IUserAnswerService
{
    public async Task<bool> CreateAsync(CreateUserAnswerDto dto)
    {
        var testAttempt = await unitOfWork.UserTestAttempts.SelectAsync(ta => ta.Id == dto.UserTestAttemptId);
        if (testAttempt is null) throw new MilliyMockException(404, "Test attempt not found");

        var selectedOption = await unitOfWork.Options.SelectAsync(o => o.Id == dto.SelectedOptionId);
        if (selectedOption is null) throw new MilliyMockException(404, "Option not found");
        
        var answer = mapper.Map<UserAnswer>(dto);
        await unitOfWork.UserAnswer.InsertAsync(answer);
        return await unitOfWork.UserAnswer.SaveAsync();
    }

    public async Task<bool> UpdateAsync(UpdateUserAnswerDto dto)
    {
        //var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

        var userAnswer = await unitOfWork.UserAnswer
            .SelectAll(ua => ua.QuestionId == dto.QuestionId)
            .Include(ua => ua.UserTestAttempt)
            .FirstOrDefaultAsync();
        if (userAnswer is null) return false;
        
        //if (userId != userAnswer.UserTestAttempt.UserId)
            //throw new MilliyMockException(401, "UnAuthorized");

        userAnswer = mapper.Map<UserAnswer>(dto);
        return await unitOfWork.UserAnswer.SaveAsync();
    }
}