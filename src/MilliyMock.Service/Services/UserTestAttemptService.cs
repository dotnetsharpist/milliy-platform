using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.TempUsers;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Dtos.UserTestAttempt;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class UserTestAttemptService(
    IUnitOfWork unitOfWork, 
    ITransactionService transactionService,
    IMapper mapper,
    ILogger<UserTestAttemptService> logger) : IUserTestAttemptService
{
    public async Task<StartTestResultDto> StartTestAsync(CreateUserTestAttemptDto dto)
    {
        try
        {
            logger.LogInformation("Creating test attempt for test {testId}", dto.TestId);

            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException();
            var user = await unitOfWork.Users.SelectAsync(u => u.Id == userId);

            if (user is null) throw new MilliyMockException(409, "Unauthorized");
            
            var test = await unitOfWork.Tests.SelectAll(t => t.Id == dto.TestId && !t.IsDeleted)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Options)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Translations)
                .FirstOrDefaultAsync();

            if (test is null) throw new MilliyMockException(404, "Test not found");
            

            if (test.IsPremium)
            {
                var result = await transactionService.ApplyTransactionAsync(userId, -test.Price, BalanceTransactionType.Purchase,
                    $"Purchase of test '{test.Title}'", test.Id);
                if (!result) throw new MilliyMockException(500, "Failed to apply transaction");
            }
            
            var groupedQuestions = await unitOfWork.QuestionGroups
                .SelectAll(qg => qg.TestId == dto.TestId && !qg.IsDeleted)
                .Include(qg => qg.Translations)
                .Include(qg => qg.Questions).ThenInclude(q => q.Translations)
                .Include(qg => qg.Options)
                .ToListAsync();
            
            var attempt = mapper.Map<UserTestAttempt>(dto);
            attempt.StartedAt = TimeHelper.GetDateTime();
            attempt.UserId = userId;

            await unitOfWork.UserTestAttempts.InsertAsync(attempt);
            await unitOfWork.UserTestAttempts.SaveAsync();

            var attemptedTest = mapper.Map<StartTestResultDto>(test);
            attemptedTest.TestAttemptId = attempt.Id;
            attemptedTest.QuestionGroups = mapper.Map<List<QuestionGroupAttemptDto>>(groupedQuestions);


            return attemptedTest;
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating test attempt for test {testId}", dto.TestId);
            throw new MilliyMockException();
        }
    }

    public async Task<TestAttemptResultsDto> GetResultsAsync(long testAttemptId)
    {
        try
        {
            logger.LogInformation("Retrieving results for test attempt {testAttemptId}", testAttemptId);

            var attempt = await unitOfWork.UserTestAttempts
                .SelectAll(ta => ta.Id == testAttemptId && ta.AttemptStatus == AttemptStatus.Completed)
                .Include(ta => ta.UserAnswers)
                .FirstOrDefaultAsync();

            if (attempt is null) throw new MilliyMockException(404, "Attempt not found");

            var test = await unitOfWork.Tests.SelectAll(t => t.Id == attempt.TestId && !t.IsDeleted)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Options)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Translations)
                .FirstOrDefaultAsync();

            if (test is null) throw new MilliyMockException(404, "Test not found");

            var groupedQuestions = await unitOfWork.QuestionGroups
                .SelectAll(qg => qg.TestId == attempt.TestId && !qg.IsDeleted)
                .Include(qg => qg.Translations)
                .Include(qg => qg.Questions).ThenInclude(q => q.Translations)
                .Include(qg => qg.Questions).ThenInclude(q => q.Options)
                .Include(qg => qg.Options)
                .ToListAsync();

            var results = mapper.Map<TestAttemptResultsDto>(test);
            results.TestAttemptId = attempt.Id;
            results.TotalScore = attempt.TotalScore;
            results.QuestionGroups = mapper.Map<List<QuestionGroupAttemptResultDto>>(groupedQuestions);
            results.UserAnswers = mapper.Map<List<UserAnswerAttemptResultDto>>(attempt.UserAnswers);

            return results;
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving results for test attempt {testAttemptId}", testAttemptId);
            throw new MilliyMockException();
        }
    }

    public async Task<AttemptedTestResultDto> SubmitTest(long testAttemptId)
    {
        try
        {
            logger.LogInformation("Submitting test attempt for testAttemptId {testAttemptId}", testAttemptId);
            //var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var testAttempt = await unitOfWork.UserTestAttempts
                .SelectAll(ta => ta.Id == testAttemptId /*&& ta.UserId == userId*/)
                .Include(ta => ta.UserAnswers)
                .Include(ta => ta.TempUser)
                .FirstOrDefaultAsync();

            if (testAttempt is null)
                throw new MilliyMockException(404, "No test attempt found");

            if (testAttempt.AttemptStatus == AttemptStatus.Completed)
                throw new MilliyMockException(400, "Test is already finished");

            // Load questions with options
            var questions = await unitOfWork.Questions
                .SelectAll(q => q.TestId == testAttempt.TestId && !q.IsDeleted)
                .Include(q => q.Options)
                .Include(q => q.Translations)
                .Include(q => q.QuestionGroup).ThenInclude(qg => qg.Translations)
                .Include(q => q.QuestionGroup).ThenInclude(g => g.Options)
                .Include(q => q.QuestionGroup).ThenInclude(g => g.QuestionExplanation).ThenInclude(qe => qe.Translations)
                .Include(q => q.QuestionExplanation).ThenInclude(qe => qe.Translations)
                .ToListAsync();
            
            decimal totalScore = 0;
            var correctCount = 0;
            var incorrectCount = 0;
            decimal maxScore = 0;

            foreach (var question in questions)
            {
                var userAnswer = testAttempt.UserAnswers
                    .FirstOrDefault(ua => ua.QuestionId == question.Id);

                if (userAnswer == null)
                    continue;

                var isCorrect = false;

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (question.Type)
                {
                    case QuestionType.MultipleChoice:
                    {
                        if (userAnswer.SelectedOptionId != null)
                        {
                            var option = question.Options
                                .FirstOrDefault(o => o.Id == userAnswer.SelectedOptionId);

                            if (option != null && option.IsCorrect)
                                isCorrect = true;
                        }

                        break;
                    }

                    case QuestionType.Matching:
                    {
                        if (userAnswer.SelectedOptionId != null && question.QuestionGroup != null)
                        {
                            var option = question.QuestionGroup.Options
                                .FirstOrDefault(o => o.Id == userAnswer.SelectedOptionId);

                            // IMPORTANT:
                            // Matching correctness must still be defined per option
                            if (option is { IsCorrect: true })
                                isCorrect = true;
                        }

                        break;
                    }

                    case QuestionType.FreeAnswer:
                    {
                        if (!string.IsNullOrWhiteSpace(userAnswer.TextAnswer) &&
                            !string.IsNullOrWhiteSpace(question.CorrectAnswer))
                        {
                            isCorrect = Normalize(userAnswer.TextAnswer)
                                        == Normalize(question.CorrectAnswer);
                        }

                        break;
                    }
                }

                if (isCorrect)
                {
                    totalScore += question.Score;
                    correctCount++;
                }
                else incorrectCount++;

                maxScore += question.Score;
            }

            testAttempt.TotalScore = totalScore;
            testAttempt.AttemptStatus = AttemptStatus.Completed;
            testAttempt.FinishedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync();

            return new AttemptedTestResultDto()
            {
                CorrectCount = correctCount,
                IncorrectCount = incorrectCount,
                MaxScore = maxScore,
                TotalScore = totalScore,
                TempUser = mapper.Map<TempUserResultDto>(testAttempt.TempUser),
                UserAnswers = mapper.Map<List<UserAnswerResultDto>>(testAttempt.UserAnswers),
                Questions = mapper.Map<List<QuestionResultDto>>(questions)
            };
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting test attempt for test {testAttemptId}", testAttemptId);
            throw new MilliyMockException();
        }
    }
    
    public async Task<List<UserTestAttemptResultDto>> GetByUserId()
    {
        try
        {
            logger.LogInformation("Retrieving test attempts for user {userId}", HttpContextHelper.UserId);
            var userId = HttpContextHelper.UserId;
            if (userId is null) throw new MilliyMockException();

            var attempts = await unitOfWork.UserTestAttempts
                .SelectAll(a => a.UserId == userId && a.AttemptStatus == AttemptStatus.Completed)
                .ToListAsync();

            return mapper.Map<List<UserTestAttemptResultDto>>(attempts);
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving test attempts for user {userId}", HttpContextHelper.UserId);
            throw new MilliyMockException();
        }
    }

    public async Task<UserTestAttemptResultDto> GetById(long testAttemptId)
    {
        try
        {
            logger.LogInformation("Retrieving full test attempt for {userId} and testAttempt {testAttemptId}",
                HttpContextHelper.UserId, testAttemptId);

            var attempt = await unitOfWork.UserTestAttempts.SelectAll(a =>
                    a.Id == testAttemptId && a.AttemptStatus == AttemptStatus.Completed)
                .Include(a => a.UserAnswers)
                .FirstOrDefaultAsync();

            if (attempt == null) throw new MilliyMockException(404, "Attempt not found");

            return mapper.Map<UserTestAttemptResultDto>(attempt);
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving in-progress test attempts for user {userId} and test {testAttemptId}",
                HttpContextHelper.UserId, testAttemptId);
            throw new MilliyMockException();
        }
    }

    public async Task<UserTestAttemptResultDto?> GetByTestId(long testId)
    {
        var attempt = await unitOfWork.UserTestAttempts.SelectAll(a =>
                a.TestId == testId)
            .Include(a => a.UserAnswers)
            .FirstOrDefaultAsync();
        
        return mapper.Map<UserTestAttemptResultDto>(attempt);
    }

    public async Task<UserTestAttemptResultDto> GetProgressAsync(long testId)
    {
        try
        {
            logger.LogInformation("Retrieving in-progress test attempts for user {userId} and test {testId}",
                HttpContextHelper.UserId, testId);
            var userId = HttpContextHelper.UserId;
            if (userId is null) throw new MilliyMockException();

            var attempt = await unitOfWork.UserTestAttempts.SelectAll(a =>
                    a.UserId == userId && a.TestId == testId && a.AttemptStatus != AttemptStatus.Completed)
                .Include(a => a.UserAnswers)
                .FirstOrDefaultAsync();

            return mapper.Map<UserTestAttemptResultDto>(attempt);
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving in-progress test attempts for user {userId} and test {testId}",
                HttpContextHelper.UserId, testId);
            throw new MilliyMockException();
        }
    }

    private static string Normalize(string input)
    {
        return input.Trim().ToLower();
    }
}