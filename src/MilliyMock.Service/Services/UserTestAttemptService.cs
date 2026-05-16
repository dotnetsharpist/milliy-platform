using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Dtos.UserTestAttempt;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class UserTestAttemptService(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    ILogger<UserTestAttemptService> logger) : IUserTestAttemptService
{
    public async Task<UserTestAttemptResultDto> CreateAsync(CreateUserTestAttemptDto dto)
    {
        try
        {
            logger.LogInformation("Creating test attempt for test {testId}", dto.TestId);
            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var ongoingAttempt = await unitOfWork.UserTestAttempts
                .SelectAsync(a =>
                    a.UserId == userId && a.TestId == dto.TestId && a.AttemptStatus != AttemptStatus.Completed);

            //if (ongoingAttempt != null)
                //throw new MilliyMockException(409, "Already have an active test twin.");

            var test = await unitOfWork.Tests.SelectAsync(t => t.Id == dto.TestId);
            if (test is null) throw new MilliyMockException(404, "Test not found");

            var attempt = mapper.Map<UserTestAttempt>(dto);
            attempt.UserId = userId;

            await unitOfWork.UserTestAttempts.InsertAsync(attempt);
            await unitOfWork.UserTestAttempts.SaveAsync();

            return mapper.Map<UserTestAttemptResultDto>(attempt);
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

    public async Task<AttemptedTestResultDto> SubmitTest(long testAttemptId)
    {
        try
        {
            logger.LogInformation("Submitting test attempt for testAttemptId {testAttemptId}", testAttemptId);
            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var testAttempt = await unitOfWork.UserTestAttempts
                .SelectAll(ta => ta.Id == testAttemptId && ta.UserId == userId)
                .Include(ta => ta.UserAnswers)
                .FirstOrDefaultAsync();

            if (testAttempt is null)
                throw new MilliyMockException(404, "No test attempt found");

            if (testAttempt.AttemptStatus == AttemptStatus.Completed)
                throw new MilliyMockException(400, "Test is already finished");

            // Load questions with options
            var questions = await unitOfWork.Questions
                .SelectAll(q => q.TestId == testAttempt.TestId)
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

    public async Task<bool> PauseTest(long testId)
    {
        try
        {
            logger.LogInformation("Pausing test attempt for test {testId}", testId);
            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var testAttempt =
                await unitOfWork.UserTestAttempts.SelectAsync(ta => ta.TestId == testId && ta.UserId == userId);

            if (testAttempt is null)
                throw new MilliyMockException(404, "No test attempt found");

            if (testAttempt.AttemptStatus == AttemptStatus.Completed)
                throw new MilliyMockException(400, "Test is already finished");

            testAttempt.AttemptStatus = AttemptStatus.Paused;
            return await unitOfWork.UserTestAttempts.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error pausing test attempt for test {testId}", testId);
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

            var attempt = await unitOfWork.UserTestAttempts
                .SelectAll(a => a.UserId == userId && a.AttemptStatus == AttemptStatus.Completed)
                .ToListAsync();

            return mapper.Map<List<UserTestAttemptResultDto>>(attempt);
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