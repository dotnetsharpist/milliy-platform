using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.UserTestAttempt;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class UserTestAttemptService(IUnitOfWork unitOfWork, IMapper mapper) : IUserTestAttemptService
{
    public async Task<bool> CreateAsync(CreateUserTestAttemptDto dto)
    {
        var attempt = mapper.Map<UserTestAttempt>(dto);
        await unitOfWork.UserTestAttempts.InsertAsync(attempt);
        return await unitOfWork.UserTestAttempts.SaveAsync();
    }

    public async Task<List<UserTestAttemptResultDto>> GetByUserId()
    {
        var userId = HttpContextHelper.UserId;
        if (userId is null) throw new MilliyMockException();

        var attempt = await unitOfWork.UserTestAttempts.SelectAll(a => a.UserId == userId)
            .Include(a => a.UserAnswers)
            .ToListAsync();

        return mapper.Map<List<UserTestAttemptResultDto>>(attempt);
    }
}