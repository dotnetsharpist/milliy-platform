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

            return await BuildAttemptResultsAsync(attempt);
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
            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var testAttempt = await unitOfWork.UserTestAttempts
                .SelectAll(ta => ta.Id == testAttemptId /*&& ta.UserId == userId*/)
                .Include(ta => ta.UserAnswers)
                .Include(ta => ta.TempUser)
                .FirstOrDefaultAsync();

            if (testAttempt is null)
                throw new MilliyMockException(404, "No test attempt found");

            if (testAttempt.UserId != userId) throw new MilliyMockException();

            if (testAttempt.AttemptStatus == AttemptStatus.Completed)
                throw new MilliyMockException(400, "Test is already finished");

            // Grading only needs each question's options (with IsCorrect) and, for
            // Matching, the group's options. Translations/explanations are loaded
            // by get-results for the review screen, not here — submit just scores.
            var questions = await unitOfWork.Questions
                .SelectAll(q => q.TestId == testAttempt.TestId && !q.IsDeleted)
                .Include(q => q.Options)
                .Include(q => q.QuestionGroup).ThenInclude(g => g.Options)
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
                        isCorrect = MatchesFreeAnswer(userAnswer.TextAnswer, question.CorrectAnswer);

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

            // submit is a lean command: grade + mark completed, return only the
            // score summary (built from data already loaded for grading). The
            // client then reads the full review payload from get-results, so we
            // don't duplicate that build here.
            return new AttemptedTestResultDto
            {
                CorrectCount = correctCount,
                IncorrectCount = incorrectCount,
                MaxScore = maxScore,
                TotalScore = totalScore,
                TempUser = mapper.Map<TempUserResultDto>(testAttempt.TempUser)
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

            return await unitOfWork.UserTestAttempts
                .SelectAll(a => a.UserId == userId && a.AttemptStatus == AttemptStatus.Completed)
                .Select(a => new UserTestAttemptResultDto
                {
                    Id = a.Id,
                    UserId = a.UserId ?? 0,
                    TestId = a.TestId,
                    StartedAt = a.StartedAt,
                    FinishedAt = a.FinishedAt,
                    TotalScore = a.TotalScore,
                    Test = new TestResultDto
                    {
                        Id = a.Test.Id,
                        Title = a.Test.Title,
                        Description = a.Test.Description,
                        Subject = a.Test.Subject,
                        IsPremium = a.Test.IsPremium,
                        Price = a.Test.Price,
                        DurationMinutes = a.Test.DurationMinutes,

                        // Pay-per-attempt: "purchased" means a paid run is currently available
                        // (more purchases than completed attempts). Flips back to false once the
                        // paid run is finished, prompting another purchase for the next attempt.
                        IsPurchased = !a.Test.IsPremium || unitOfWork.TransactionHistories
                            .SelectAll()
                            .Count(th => th.UserId == userId
                                         && th.TestId == a.Test.Id
                                         && th.Type == BalanceTransactionType.Purchase)
                            > unitOfWork.UserTestAttempts
                            .SelectAll()
                            .Count(att => att.UserId == userId
                                          && att.TestId == a.Test.Id
                                          && att.AttemptStatus == AttemptStatus.Completed),

                        QuestionCount = unitOfWork.Questions
                                            .SelectAll()
                                            .Where(q => !q.IsDeleted && q.TestId == a.Test.Id)
                                            .Where(q => !(q.QuestionGroupId != null && q.Type == QuestionType.FreeAnswer))
                                            .Count()
                                        +
                                        unitOfWork.Questions
                                            .SelectAll()
                                            .Where(q => !q.IsDeleted && q.TestId == a.Test.Id)
                                            .Where(q => q.QuestionGroupId != null && q.Type == QuestionType.FreeAnswer)
                                            .Select(q => q.QuestionGroupId)
                                            .Distinct()
                                            .Count(),

                        AttemptCount = unitOfWork.UserTestAttempts
                            .SelectAll()
                            .Count(att => att.TestId == a.Test.Id),

                        Status = a.Test.Status
                    }
                })
                .ToListAsync();
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

    // Shared graded-results builder for a completed attempt. Both GetResultsAsync
    // (read-only) and SubmitTest (right after grading) return this same shape, so
    // the client renders the explanation screen from a single response. The
    // attempt must already be loaded with its UserAnswers.
    private async Task<TestAttemptResultsDto> BuildAttemptResultsAsync(UserTestAttempt attempt)
    {
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

    // A FreeAnswer is correct when it matches any of the author's accepted forms.
    // CorrectAnswer may list several, separated by "||" — typically a LaTeX form
    // and a plain form of the same value, e.g. "\frac{4}{7}||4/7". A student who
    // types either form should be marked correct, so we compare against each
    // variant, not the joined string.
    private static bool MatchesFreeAnswer(string? userAnswer, string? correctAnswer)
    {
        if (string.IsNullOrWhiteSpace(userAnswer) || string.IsNullOrWhiteSpace(correctAnswer))
            return false;

        var normalizedUser = Normalize(userAnswer);
        if (normalizedUser.Length == 0)
            return false;

        return correctAnswer
            .Split("||", StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize)
            .Any(accepted => accepted.Length > 0 && accepted == normalizedUser);
    }

    // Case- and whitespace-insensitive. LaTeX spacing isn't significant
    // (\frac {4}{7} == \frac{4}{7}) and the editor pads inline math with spaces,
    // so we drop all whitespace rather than only trimming the ends.
    private static string Normalize(string input)
    {
        return new string(input
            .Where(character => !char.IsWhiteSpace(character))
            .Select(char.ToLowerInvariant)
            .ToArray());
    }
}