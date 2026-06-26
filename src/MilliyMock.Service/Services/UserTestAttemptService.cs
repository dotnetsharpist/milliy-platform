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

    public async Task<ResumeAttemptDto> ResumeAsync(long testAttemptId)
    {
        try
        {
            logger.LogInformation("Resuming test attempt {testAttemptId}", testAttemptId);

            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var attempt = await unitOfWork.UserTestAttempts
                .SelectAll(ta => ta.Id == testAttemptId)
                .Include(ta => ta.UserAnswers)
                .FirstOrDefaultAsync();

            if (attempt is null) throw new MilliyMockException(404, "Attempt not found");
            if (attempt.UserId != userId) throw new MilliyMockException();

            var test = await unitOfWork.Tests.SelectAll(t => t.Id == attempt.TestId && !t.IsDeleted)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Options)
                .Include(t => t.Questions.Where(q => q.QuestionGroupId == null && !q.IsDeleted)).ThenInclude(q => q.Translations)
                .FirstOrDefaultAsync();

            if (test is null) throw new MilliyMockException(404, "Test not found");

            var groupedQuestions = await unitOfWork.QuestionGroups
                .SelectAll(qg => qg.TestId == attempt.TestId && !qg.IsDeleted)
                .Include(qg => qg.Translations)
                .Include(qg => qg.Questions).ThenInclude(q => q.Translations)
                .Include(qg => qg.Options)
                .ToListAsync();

            // The window is StartedAt + the test's duration. Compare against the
            // same UTC+5 clock StartedAt was stamped with. A non-positive remainder
            // means the window has elapsed: finish the attempt now so a returning
            // user gets a graded result instead of a resumable, already-late test.
            var deadline = attempt.StartedAt.AddMinutes(test.DurationMinutes);
            var remaining = deadline - TimeHelper.GetDateTime();
            var remainingSeconds = (int)Math.Max(0, Math.Floor(remaining.TotalSeconds));

            if (attempt.AttemptStatus != AttemptStatus.Completed && remainingSeconds == 0)
            {
                CompleteAttempt(attempt);
                await unitOfWork.SaveChangesAsync();
            }

            var resume = mapper.Map<ResumeAttemptDto>(test);
            resume.TestAttemptId = attempt.Id;
            resume.TestId = attempt.TestId;
            resume.RemainingSeconds = remainingSeconds;
            resume.IsCompleted = attempt.AttemptStatus == AttemptStatus.Completed;
            resume.QuestionGroups = mapper.Map<List<QuestionGroupAttemptDto>>(groupedQuestions);
            resume.UserAnswers = mapper.Map<List<UserAnswerAttemptResultDto>>(attempt.UserAnswers);

            return resume;
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resuming test attempt {testAttemptId}", testAttemptId);
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

    public async Task<SubmitTestResultDto> SubmitTest(long testAttemptId)
    {
        try
        {
            logger.LogInformation("Submitting test attempt for testAttemptId {testAttemptId}", testAttemptId);
            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var testAttempt = await unitOfWork.UserTestAttempts
                .SelectAll(ta => ta.Id == testAttemptId)
                .FirstOrDefaultAsync();

            if (testAttempt is null)
                throw new MilliyMockException(404, "No test attempt found");

            if (testAttempt.UserId != userId) throw new MilliyMockException();

            if (testAttempt.AttemptStatus == AttemptStatus.Completed)
                throw new MilliyMockException(400, "Test is already finished");

            // Submit is a pure state transition: mark the attempt finished. Scoring
            // (TotalScore + correct/incorrect counts) is computed lazily by
            // get-results, so finishing an attempt — including an expired one —
            // never needs to load the questions or grade here.
            CompleteAttempt(testAttempt);

            await unitOfWork.SaveChangesAsync();

            return new SubmitTestResultDto
            {
                TestAttemptId = testAttempt.Id,
                AttemptStatus = testAttempt.AttemptStatus
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

    // Builds the graded review payload for a completed attempt. Submit only marks
    // the attempt finished; the score and the correct/incorrect tallies are
    // derived here, on read, from the loaded questions and the user's answers — so
    // there is one authoritative grading path feeding both the certificate and the
    // results list. The attempt must already be loaded with its UserAnswers.
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
        results.QuestionGroups = mapper.Map<List<QuestionGroupAttemptResultDto>>(groupedQuestions);
        results.UserAnswers = mapper.Map<List<UserAnswerAttemptResultDto>>(attempt.UserAnswers);

        var grade = GradeAttempt(EnumerateGradableQuestions(test, groupedQuestions), attempt.UserAnswers);
        results.TotalScore = grade.TotalScore;
        results.MaxScore = grade.MaxScore;
        results.CorrectCount = grade.CorrectCount;
        results.IncorrectCount = grade.IncorrectCount;

        // Cache the score on the attempt so the history list (GetByUserId) reflects
        // it without re-grading. Idempotent — recomputing yields the same value.
        if (attempt.TotalScore != grade.TotalScore)
        {
            attempt.TotalScore = grade.TotalScore;
            await unitOfWork.SaveChangesAsync();
        }

        return results;
    }

    // Marks an attempt finished. Shared by submit and the expiry path so both set
    // the status, finish time and elapsed span the same way. Uses the same UTC+5
    // clock as StartedAt so TimeSpent is a real duration.
    private static void CompleteAttempt(UserTestAttempt attempt)
    {
        attempt.AttemptStatus = AttemptStatus.Completed;
        attempt.FinishedAt = TimeHelper.GetDateTime();
        attempt.TimeSpent = attempt.FinishedAt.Value - attempt.StartedAt;
    }

    // Flattens a test's gradable questions to (question, owning group) pairs:
    // ungrouped questions carry a null group; grouped sub-questions carry their
    // parent so Matching correctness can be resolved from the group's options.
    private static IEnumerable<(Question Question, QuestionGroup? Group)> EnumerateGradableQuestions(
        Test test, IEnumerable<QuestionGroup> groups)
    {
        foreach (var question in test.Questions)
            yield return (question, null);

        foreach (var group in groups)
            foreach (var question in group.Questions)
                yield return (question, group);
    }

    // Single grading pass over every gradable question. MaxScore counts all
    // questions (the achievable total); correct/incorrect count only answered
    // ones, so an omitted question is neither. Matching grades against the parent
    // group's options; FreeAnswer uses MatchesFreeAnswer.
    private static (decimal TotalScore, decimal MaxScore, int CorrectCount, int IncorrectCount) GradeAttempt(
        IEnumerable<(Question Question, QuestionGroup? Group)> questions,
        IEnumerable<UserAnswer> userAnswers)
    {
        var answerByQuestionId = userAnswers
            .GroupBy(ua => ua.QuestionId)
            .ToDictionary(group => group.Key, group => group.First());

        decimal totalScore = 0;
        decimal maxScore = 0;
        var correctCount = 0;
        var incorrectCount = 0;

        foreach (var (question, group) in questions)
        {
            maxScore += question.Score;

            if (!answerByQuestionId.TryGetValue(question.Id, out var userAnswer))
                continue;

            var isCorrect = question.Type switch
            {
                QuestionType.MultipleChoice =>
                    userAnswer.SelectedOptionId is { } mcOption &&
                    question.Options.Any(o => o.Id == mcOption && o.IsCorrect),
                QuestionType.Matching =>
                    userAnswer.SelectedOptionId is { } matchOption && group != null &&
                    group.Options.Any(o => o.Id == matchOption && o.IsCorrect),
                QuestionType.FreeAnswer =>
                    MatchesFreeAnswer(userAnswer.TextAnswer, question.CorrectAnswer),
                _ => false
            };

            if (isCorrect)
            {
                totalScore += question.Score;
                correctCount++;
            }
            else
            {
                incorrectCount++;
            }
        }

        return (totalScore, maxScore, correctCount, incorrectCount);
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