using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class TestService(IUnitOfWork unitOfWork, IMapper mapper) : ITestService
{
    public async Task<bool> CreateAsync(CreateTestDto dto)
    {
        var test = mapper.Map<Test>(dto);
        test.CreatedBy = HttpContextHelper.UserId;
        await unitOfWork.Tests.InsertAsync(test);
        return await unitOfWork.Tests.SaveAsync();
    }

    public async Task<List<TestResultDto>> GetAllAsync()
    {
        return await unitOfWork.Tests
            .SelectAll()
            .Select(test => new TestResultDto
            {
                Id = test.Id,
                Title = test.Title,

                QuestionCount = unitOfWork.Questions
                    .SelectAll()
                    .Count(q => q.TestId == test.Id),

                AttemptCount = unitOfWork.UserTestAttempts
                    .SelectAll()
                    .Count(a => a.TestId == test.Id)
            })
            .ToListAsync();
    }
}