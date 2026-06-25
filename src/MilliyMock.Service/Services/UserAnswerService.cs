using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.UserAnswers;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class UserAnswerService(IUnitOfWork unitOfWork, IMapper mapper) : IUserAnswerService
{
    /// <summary>
    /// Idempotent "set the answer for this question in this attempt". Inserts on first
    /// save, updates in place afterwards — safe to call on every option change. Backed by
    /// the unique index on (UserTestAttemptId, QuestionId). Rejects writes once the attempt
    /// is no longer active (submitted / abandoned / expired).
    /// </summary>
    public async Task<bool> SetAnswerAsync(CreateUserAnswerDto dto)
    {
        // Exactly one of (option, text) must be provided.
        var hasOption = dto.SelectedOptionId is not null;
        var hasText = !string.IsNullOrWhiteSpace(dto.TextAnswer);
        if (hasOption == hasText)
            throw new MilliyMockException(400, "Provide exactly one of SelectedOptionId or TextAnswer");

        // One read: the attempt (for its status) together with its existing answer to this
        // question. The filtered Include returns a tracked entity, so the update path emits
        // a clean UPDATE without a second query.
        var attempt = await unitOfWork.UserTestAttempts
            .SelectAll(ta => ta.Id == dto.UserTestAttemptId)
            .Include(ta => ta.UserAnswers.Where(ua => ua.QuestionId == dto.QuestionId))
            .FirstOrDefaultAsync();

        if (attempt is null)
            throw new MilliyMockException(404, "Test attempt not found");

        // Only an active attempt can be answered; whitelist so any future terminal
        // status is blocked by default.
        if (attempt.AttemptStatus is not (AttemptStatus.InProgress or AttemptStatus.Paused))
            throw new MilliyMockException(409, "This attempt is no longer active and cannot be answered");

        var existing = attempt.UserAnswers.FirstOrDefault();

        if (existing is null)
        {
            // Option validity is enforced by the FK; a bad id surfaces on save (translated below).
            await unitOfWork.UserAnswer.InsertAsync(mapper.Map<UserAnswer>(dto));
        }
        else
        {
            existing.SelectedOptionId = dto.SelectedOptionId;
            existing.TextAnswer = dto.TextAnswer;
            existing.UpdatedAt = TimeHelper.GetDateTime();
        }

        try
        {
            return await unitOfWork.UserAnswer.SaveAsync();
        }
        catch (DbUpdateException)
        {
            throw new MilliyMockException(404, "Option not found");
        }
    }
}