using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Practice;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class PracticeService(
    IUnitOfWork unitOfWork,
    ITransactionService transactionService,
    ILogger<PracticeService> logger) : IPracticeService
{
    // Business rules — see docs/MASHQ_PRACTICE_SPEC.md
    private const int FreeDailyLimit = 10;
    private const int ExtraPerPurchase = 10;
    private const int PriceTanga = 2;

    public async Task<List<PracticeQuestionResultDto>> GetQuestionsAsync(
        SubjectType subject, int? grade, PracticeDifficulty? difficulty, string? topic, string? status, int count)
    {
        var userId = CurrentUserId();
        count = Math.Clamp(count, 1, 20);

        var query = unitOfWork.PracticeQuestions
            .SelectAll(q => q.IsActive && q.Subject == subject);

        // Grade-less questions are universal — they match any grade filter.
        if (grade.HasValue)
            query = query.Where(q => q.Grade == grade.Value || q.Grade == null);

        if (difficulty.HasValue)
            query = query.Where(q => q.Difficulty == difficulty.Value);

        if (!string.IsNullOrWhiteSpace(topic))
            query = query.Where(q => q.Topic == topic);

        var attempts = unitOfWork.PracticeAttempts.SelectAll(a => a.UserId == userId);
        var saved = unitOfWork.PracticeSavedQuestions.SelectAll(s => s.UserId == userId);

        // Status is judged by the LATEST attempt (students may retry mistakes).
        query = status switch
        {
            "unanswered" => query.Where(q => !attempts.Any(a => a.PracticeQuestionId == q.Id)),
            "correct" => query.Where(q => attempts
                .Where(a => a.PracticeQuestionId == q.Id)
                .OrderByDescending(a => a.Id)
                .Select(a => (bool?)a.IsCorrect)
                .FirstOrDefault() == true),
            "incorrect" => query.Where(q => attempts
                .Where(a => a.PracticeQuestionId == q.Id)
                .OrderByDescending(a => a.Id)
                .Select(a => (bool?)a.IsCorrect)
                .FirstOrDefault() == false),
            "saved" => query.Where(q => saved.Any(s => s.PracticeQuestionId == q.Id)),
            _ => query,
        };

        return await query
            .OrderBy(q => EF.Functions.Random())
            .Take(count)
            .Select(q => new PracticeQuestionResultDto
            {
                Id = q.Id,
                Subject = (int)q.Subject,
                Grade = q.Grade,
                Difficulty = (int)q.Difficulty,
                Topic = q.Topic,
                Text = q.Text,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                IsSaved = saved.Any(s => s.PracticeQuestionId == q.Id),
                LastResult = attempts
                    .Where(a => a.PracticeQuestionId == q.Id)
                    .OrderByDescending(a => a.Id)
                    .Select(a => a.IsCorrect ? "correct" : "incorrect")
                    .FirstOrDefault(),
            })
            .ToListAsync();
    }

    public async Task<PracticeAnswerResultDto> AnswerAsync(PracticeAnswerDto dto)
    {
        var userId = CurrentUserId();

        var letter = dto.Letter?.Trim().ToUpperInvariant();
        if (letter is not ("A" or "B" or "C" or "D"))
            throw new MilliyMockException(400, "Letter must be one of A, B, C, D");

        // Quota is consumed on answer submit, not on question fetch.
        var quota = await GetQuotaAsync();
        if (quota.Remaining <= 0)
            throw new MilliyMockException(402, "quota_exhausted");

        var question = await unitOfWork.PracticeQuestions
                           .SelectAsync(q => q.Id == dto.QuestionId && q.IsActive)
                       ?? throw new MilliyMockException(404, "Question not found");

        var isCorrect = question.CorrectLetter == letter;

        await unitOfWork.PracticeAttempts.InsertAsync(new PracticeAttempt
        {
            UserId = userId,
            PracticeQuestionId = question.Id,
            ChosenLetter = letter,
            IsCorrect = isCorrect,
            CreatedBy = userId,
        });
        await unitOfWork.SaveChangesAsync();

        return new PracticeAnswerResultDto
        {
            IsCorrect = isCorrect,
            CorrectLetter = question.CorrectLetter,
            ExplanationTitle = question.ExplanationTitle,
            Explanation = question.Explanation,
            QuotaRemaining = quota.Remaining - 1,
        };
    }

    public async Task<PracticeQuotaResultDto> GetQuotaAsync()
    {
        var userId = CurrentUserId();

        // Day boundary in Tashkent local time — CreatedAt is written via TimeHelper,
        // so both sides of the comparison share the same offset.
        var today = TimeHelper.GetDateOnly();
        var dayStart = today.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var used = await unitOfWork.PracticeAttempts
            .SelectAll(a => a.UserId == userId && a.CreatedAt >= dayStart && a.CreatedAt < dayEnd)
            .CountAsync();

        var extra = await unitOfWork.PracticeQuotaPurchases
            .SelectAll(p => p.UserId == userId && p.Day == today)
            .SumAsync(p => (int?)p.ExtraQuestions) ?? 0;

        return new PracticeQuotaResultDto
        {
            FreeLimit = FreeDailyLimit,
            Used = used,
            Extra = extra,
            Remaining = Math.Max(0, FreeDailyLimit + extra - used),
        };
    }

    public async Task<PracticeQuotaResultDto> PurchaseQuotaAsync()
    {
        var userId = CurrentUserId();
        logger.LogInformation("Practice quota purchase for user {userId}", userId);

        // Throws 400 "Insufficient balance" when the user can't afford it.
        var applied = await transactionService.ApplyTransactionAsync(
            userId, -PriceTanga, BalanceTransactionType.Purchase, "Mashq: qo'shimcha 10 savol", null);
        if (!applied) throw new MilliyMockException();

        await unitOfWork.PracticeQuotaPurchases.InsertAsync(new PracticeQuotaPurchase
        {
            UserId = userId,
            Day = TimeHelper.GetDateOnly(),
            ExtraQuestions = ExtraPerPurchase,
            TangaSpent = PriceTanga,
            CreatedBy = userId,
        });
        await unitOfWork.SaveChangesAsync();

        return await GetQuotaAsync();
    }

    public async Task<PracticeSaveResultDto> ToggleSaveAsync(long questionId)
    {
        var userId = CurrentUserId();

        var existing = await unitOfWork.PracticeSavedQuestions
            .SelectAsync(s => s.UserId == userId && s.PracticeQuestionId == questionId);

        if (existing is not null)
        {
            await unitOfWork.PracticeSavedQuestions.DeleteAsync(s => s.Id == existing.Id);
            await unitOfWork.SaveChangesAsync();
            return new PracticeSaveResultDto { IsSaved = false };
        }

        var question = await unitOfWork.PracticeQuestions
                           .SelectAsync(q => q.Id == questionId && q.IsActive)
                       ?? throw new MilliyMockException(404, "Question not found");

        await unitOfWork.PracticeSavedQuestions.InsertAsync(new PracticeSavedQuestion
        {
            UserId = userId,
            PracticeQuestionId = question.Id,
            CreatedBy = userId,
        });
        await unitOfWork.SaveChangesAsync();

        return new PracticeSaveResultDto { IsSaved = true };
    }

    public async Task<List<PracticeTopicResultDto>> GetTopicsAsync(SubjectType subject)
    {
        return await unitOfWork.PracticeTopics
            .SelectAll(t => t.Subject == subject && !t.IsDeleted)
            .OrderBy(t => t.Order)
            .Select(t => new PracticeTopicResultDto
            {
                Slug = t.Slug,
                Name = t.Name,
                Section = t.Section,
            })
            .ToListAsync();
    }

    private static long CurrentUserId()
        => HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");
}
