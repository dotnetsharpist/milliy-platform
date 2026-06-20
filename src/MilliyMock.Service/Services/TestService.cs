using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
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

    public async Task<List<TestResultDto>> GetAllAsync(SubjectType? subject)
    {
        var userRole = HttpContextHelper.UserRole;
        var userId = HttpContextHelper.UserId;
        var isAdmin = userRole is "Admin" or "SuperAdmin";

        return await unitOfWork.Tests
            .SelectAll(t => !t.IsDeleted)
            .Where(t => subject == null || t.Subject == subject)
            .Where(t => userRole != "Admin" && userRole != "SuperAdmin" ? t.Status == TestStatus.Published : true)
            .Select(test => new TestResultDto
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,
                Subject = test.Subject,
                IsPremium = test.IsPremium,
                Price = test.Price,

                // Pay-per-attempt: "purchased" means a paid run is currently available
                // (more purchases than completed attempts). Flips back to false once the
                // paid run is finished, prompting another purchase for the next attempt.
                IsPurchased = !test.IsPremium || isAdmin || (userId != null
                    && unitOfWork.TransactionHistories
                        .SelectAll()
                        .Count(th => th.UserId == userId
                                     && th.TestId == test.Id
                                     && th.Type == BalanceTransactionType.Purchase)
                       > unitOfWork.UserTestAttempts
                        .SelectAll()
                        .Count(a => a.UserId == userId
                                    && a.TestId == test.Id
                                    && a.AttemptStatus == AttemptStatus.Completed)),

                QuestionCount = unitOfWork.Questions
                                    .SelectAll()
                                    .Where(q => !q.IsDeleted && q.TestId == test.Id)
                                    .Where(q => !(q.QuestionGroupId != null && q.Type == QuestionType.FreeAnswer))
                                    .Count()
                                +
                                unitOfWork.Questions
                                    .SelectAll()
                                    .Where(q => !q.IsDeleted && q.TestId == test.Id)
                                    .Where(q => q.QuestionGroupId != null && q.Type == QuestionType.FreeAnswer)
                                    .Select(q => q.QuestionGroupId)
                                    .Distinct()
                                    .Count(),

                AttemptCount = unitOfWork.UserTestAttempts
                    .SelectAll()
                    .Count(a => a.TestId == test.Id),

                Status = test.Status
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
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Options)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Translations)
                .FirstOrDefaultAsync();

            if (test is null) throw new MilliyMockException(404, "Test not found");

            await EnsureTestAccessAsync(test);

            var groupedQuestions = await unitOfWork.QuestionGroups
                .SelectAll(qg => qg.TestId == testId && !qg.IsDeleted)
                .Include(qg => qg.Translations)
                .Include(qg => qg.Questions).ThenInclude(q => q.Translations)
                .Include(qg => qg.Options)
                .ToListAsync();

            // mapping
            var fullTest = mapper.Map<FullTestResultDto>(test);
            fullTest.QuestionGroups = mapper.Map<List<QuestionGroupAttemptDto>>(groupedQuestions);

            return fullTest;
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting full test with id: {id}", testId);
            throw new MilliyMockException();
        }
    }

    /// <summary>
    /// Ensures the current caller is allowed to open the given test's full content.
    /// Free tests and admins always pass. For premium tests access is pay-per-attempt:
    /// every purchase unlocks exactly one run and is consumed once that run is completed,
    /// so the content stays open while a paid run is in progress (resume / refresh) but
    /// locks again afterwards until the test is purchased anew.
    /// </summary>
    private async Task EnsureTestAccessAsync(Test test)
    {
        if (!test.IsPremium) return;

        var userRole = HttpContextHelper.UserRole;
        if (userRole is "Admin" or "SuperAdmin") return;

        var userId = HttpContextHelper.UserId
                     ?? throw new MilliyMockException(402, "This is a premium test. Please sign in and purchase it.");

        var purchaseCount = await unitOfWork.TransactionHistories
            .SelectAll(th => th.UserId == userId
                             && th.TestId == test.Id
                             && th.Type == BalanceTransactionType.Purchase)
            .CountAsync();

        var completedCount = await unitOfWork.UserTestAttempts
            .SelectAll(a => a.UserId == userId
                            && a.TestId == test.Id
                            && a.AttemptStatus == AttemptStatus.Completed)
            .CountAsync();

        // Access only while there is a paid run that has not yet been completed.
        if (purchaseCount <= completedCount)
            throw new MilliyMockException(402, "This is a premium test. Please purchase it to start a new attempt.");
    }
}