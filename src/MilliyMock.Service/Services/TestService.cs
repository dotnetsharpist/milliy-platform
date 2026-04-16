using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class TestService(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    ILogger<TestService> logger) : ITestService
{
    public async Task<bool> CreateAsync(CreateTestDto dto)
    {
        var test = mapper.Map<Test>(dto);
        test.CreatedBy = HttpContextHelper.UserId;
        await unitOfWork.Tests.InsertAsync(test);
        return await unitOfWork.Tests.SaveAsync();
    }

    public async Task<bool> UpdateAsync(long testId, UpdateTestDto dto)
    {
        var test = await unitOfWork.Tests.SelectAsync(t => t.Id == testId);
        if (test is null) throw new MilliyMockException(404, "Test not found");

        mapper.Map(dto, test);
        test.UpdatedBy = HttpContextHelper.UserId;
        test.UpdatedAt = TimeHelper.GetDateTime();
        unitOfWork.Tests.Update(test);
        return await unitOfWork.Tests.SaveAsync();
    }

    public async Task<bool> DeleteAsync(long testId)
    {
        var test = await unitOfWork.Tests.SelectAsync(t => t.Id == testId);
        if (test is null) throw new MilliyMockException(404, "Test not found");

        test.IsDeleted = true;
        return await unitOfWork.Tests.SaveAsync();
    }

    public async Task<List<TestResultDto>> GetAllAsync()
    {
        return await unitOfWork.Tests
            .SelectAll(t => !t.IsDeleted)
            .Select(test => new TestResultDto
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,

                QuestionCount = unitOfWork.Questions
                    .SelectAll()
                    .Count(q => q.TestId == test.Id),

                AttemptCount = unitOfWork.UserTestAttempts
                    .SelectAll()
                    .Count(a => a.TestId == test.Id),
                TestStatus = test.Status
            })
            .ToListAsync();
    }

    public async Task<TestResultDto> GetByIdAsync(long testId)
    {
        var test = await unitOfWork.Tests.SelectAsync(t => t.Id == testId && !t.IsDeleted);
        if (test is null) throw new MilliyMockException(404, "Test not found");

        var testDto = mapper.Map<TestResultDto>(test);

        return testDto;
    }

    public async Task<FullTestResultDto> GetFullTest(long testId)
    {
        try
        {
            var test = await unitOfWork.Tests.SelectAll(t => t.Id == testId && !t.IsDeleted)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null)).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync();

            var groupedQuestions = await unitOfWork.QuestionGroups
                .SelectAll(qg => qg.TestId == testId)
                .Include(qg => qg.Questions)
                .Include(qg => qg.Options)
                .ToListAsync();

            // mapping
            var fullTest = mapper.Map<FullTestResultDto>(test);
            fullTest.QuestionGroups = mapper.Map<List<QuestionGroupAttemptDto>>(groupedQuestions);

            return fullTest;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting full test with id: {id}", testId);
            throw new MilliyMockException();
        }
    }
}