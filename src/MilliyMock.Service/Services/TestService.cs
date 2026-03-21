using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Service.Services;

public class TestService(IUnitOfWork unitOfWork, IMapper mapper) : ITestService
{
    public async Task<bool> CreateAsync(CreateTestDto dto)
    {
        var test = mapper.Map<Test>(dto);
        await unitOfWork.Tests.InsertAsync(test);
        return await unitOfWork.Tests.SaveAsync();
    }

    public async Task<List<TestResultDto>> GetAllAsync()
    {
        var tests = unitOfWork.Tests.SelectAll();
        var questions = unitOfWork.Questions.SelectAll();
        var attempts = unitOfWork.UserTestAttempts.SelectAll();

        return await tests
            .GroupJoin(questions,
                t => t.Id,
                q => q.TestId,
                (t, qs) => new { t, QuestionCount = qs.Count() })
            .GroupJoin(attempts,
                temp => temp.t.Id,
                a => a.TestId,
                (temp, atts) => new TestResultDto
                {
                    Id = temp.t.Id,
                    Title = temp.t.Title,
                    QuestionCount = temp.QuestionCount,
                    AttemptCount = atts.Count()
                })
            .ToListAsync();
    }
}