using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Tests;
using MilliyMock.Service.Dtos.UserTestAttempt;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class UserTestAttemptService(IUnitOfWork unitOfWork, IMapper mapper) : IUserTestAttemptService
{
    public async Task<UserTestAttemptResultDto> CreateAsync(CreateUserTestAttemptDto dto)
    {
        var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

        var ongoingAttempt =
            await unitOfWork.UserTestAttempts.SelectAsync(a => a.UserId == userId && a.FinishedAt == null);
        if (ongoingAttempt != null)
            throw new MilliyMockException(409, "Finish your previous test.");
            
        var test = await unitOfWork.Tests.SelectAsync(t => t.Id == dto.TestId);
        if (test is null) throw new MilliyMockException(404, "Test not found");
        
        var attempt = mapper.Map<UserTestAttempt>(dto);
        attempt.UserId = userId;
        
        await unitOfWork.UserTestAttempts.InsertAsync(attempt);
        await unitOfWork.UserTestAttempts.SaveAsync();
        
        return mapper.Map<UserTestAttemptResultDto>(attempt);
    }

    public async Task<AttemptedTestResultDto> SubmitTest()
    {
        var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");
        var testAttempt = await unitOfWork.UserTestAttempts.SelectAsync(ta => ta.UserId == userId);
        if (testAttempt is null)
            throw new MilliyMockException(404, "No test attempt found");
        if (testAttempt.FinishedAt != null)
            throw new MilliyMockException(400, "Test is already finished");

        // Load questions with options
        var questions = await unitOfWork.Questions
            .SelectAll(q => q.TestId == testAttempt.TestId)
            .Include(q => q.Options)
            .Include(q => q.QuestionGroup)
            .ThenInclude(g => g.Options)
            .ToListAsync();

        decimal totalScore = 0;
        var correctCount = 0;
        var incorectCount = 0;
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
                        if (option != null && option.IsCorrect)
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
            else incorectCount++;

            maxScore += question.Score;
        }

        testAttempt.TotalScore = totalScore;
        testAttempt.FinishedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        return new AttemptedTestResultDto()
        {
            CorrectCount = correctCount,
            IncorrectCount = incorectCount,
            MaxScore = maxScore,
            TotalScore = totalScore
        };
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
    
    private static string Normalize(string input)
    {
        return input.Trim().ToLower();
    }
}