using System.Text.RegularExpressions;
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

// Admin CRUD for the practice bank. Keeps topics and questions in sync:
// a question can only reference an existing topic of the same subject,
// renaming a topic slug cascades to its questions, and a topic with
// questions cannot be deleted.
public partial class PracticeAdminService(
    IUnitOfWork unitOfWork,
    ILogger<PracticeAdminService> logger) : IPracticeAdminService
{
    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex SlugRegex();

    // ------------------------------------------------------------- Topics

    public async Task<List<PracticeTopicAdminResultDto>> GetTopicsAsync(SubjectType? subject)
    {
        var query = unitOfWork.PracticeTopics.SelectAll(t => !t.IsDeleted);
        if (subject.HasValue)
            query = query.Where(t => t.Subject == subject.Value);

        var questions = unitOfWork.PracticeQuestions.SelectAll(q => !q.IsDeleted);

        return await query
            .OrderBy(t => t.Subject).ThenBy(t => t.Order)
            .Select(t => new PracticeTopicAdminResultDto
            {
                Id = t.Id,
                Subject = (int)t.Subject,
                Slug = t.Slug,
                Name = t.Name,
                Section = t.Section,
                Order = t.Order,
                QuestionCount = questions.Count(q => q.Subject == t.Subject && q.Topic == t.Slug),
            })
            .ToListAsync();
    }

    public async Task<PracticeTopicAdminResultDto> CreateTopicAsync(SavePracticeTopicDto dto)
    {
        var slug = NormalizeSlug(dto.Slug);
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new MilliyMockException(400, "Name is required");

        var duplicate = await unitOfWork.PracticeTopics
            .SelectAsync(t => t.Subject == dto.Subject && t.Slug == slug);
        if (duplicate is not null)
            throw new MilliyMockException(409, $"Topic '{slug}' already exists for this subject");

        var topic = await unitOfWork.PracticeTopics.InsertAsync(new PracticeTopic
        {
            Subject = dto.Subject,
            Slug = slug,
            Name = name,
            Section = NormalizeOptional(dto.Section),
            Order = dto.Order,
            CreatedBy = HttpContextHelper.UserId,
        });
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Practice topic created: {subject}/{slug}", dto.Subject, slug);
        return ToAdminDto(topic, questionCount: 0);
    }

    public async Task<PracticeTopicAdminResultDto> UpdateTopicAsync(long id, SavePracticeTopicDto dto)
    {
        var topic = await unitOfWork.PracticeTopics.SelectAsync(t => t.Id == id)
                    ?? throw new MilliyMockException(404, "Topic not found");

        var slug = NormalizeSlug(dto.Slug);
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new MilliyMockException(400, "Name is required");

        var duplicate = await unitOfWork.PracticeTopics
            .SelectAsync(t => t.Id != id && t.Subject == dto.Subject && t.Slug == slug);
        if (duplicate is not null)
            throw new MilliyMockException(409, $"Topic '{slug}' already exists for this subject");

        // Subject/slug changes cascade to questions so they stay attached.
        var oldSubject = topic.Subject;
        var oldSlug = topic.Slug;
        var refChanged = oldSubject != dto.Subject || oldSlug != slug;

        var questionCount = 0;
        if (refChanged)
        {
            var linked = await unitOfWork.PracticeQuestions
                .SelectAll(q => q.Subject == oldSubject && q.Topic == oldSlug && !q.IsDeleted)
                .ToListAsync();

            foreach (var question in linked)
            {
                question.Subject = dto.Subject;
                question.Topic = slug;
                question.UpdatedBy = HttpContextHelper.UserId;
                question.UpdatedAt = TimeHelper.GetDateTime();
                unitOfWork.PracticeQuestions.Update(question);
            }

            questionCount = linked.Count;
        }

        topic.Subject = dto.Subject;
        topic.Slug = slug;
        topic.Name = name;
        topic.Section = NormalizeOptional(dto.Section);
        topic.Order = dto.Order;
        topic.UpdatedBy = HttpContextHelper.UserId;
        topic.UpdatedAt = TimeHelper.GetDateTime();
        unitOfWork.PracticeTopics.Update(topic);
        await unitOfWork.SaveChangesAsync();

        if (!refChanged)
            questionCount = await unitOfWork.PracticeQuestions
                .SelectAll(q => q.Subject == topic.Subject && q.Topic == topic.Slug && !q.IsDeleted)
                .CountAsync();

        return ToAdminDto(topic, questionCount);
    }

    public async Task<bool> DeleteTopicAsync(long id)
    {
        var topic = await unitOfWork.PracticeTopics.SelectAsync(t => t.Id == id)
                    ?? throw new MilliyMockException(404, "Topic not found");

        var questionCount = await unitOfWork.PracticeQuestions
            .SelectAll(q => q.Subject == topic.Subject && q.Topic == topic.Slug && !q.IsDeleted)
            .CountAsync();
        if (questionCount > 0)
            throw new MilliyMockException(409,
                $"Topic has {questionCount} question(s). Move or delete them first");

        await unitOfWork.PracticeTopics.DeleteAsync(t => t.Id == id);
        return await unitOfWork.SaveChangesAsync();
    }

    // ---------------------------------------------------------- Questions

    public async Task<PagedPracticeQuestionsDto> GetQuestionsAsync(
        SubjectType? subject, string? topic, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = unitOfWork.PracticeQuestions.SelectAll(q => !q.IsDeleted);
        if (subject.HasValue) query = query.Where(q => q.Subject == subject.Value);
        if (!string.IsNullOrWhiteSpace(topic)) query = query.Where(q => q.Topic == topic);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new PracticeQuestionAdminResultDto
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
                CorrectLetter = q.CorrectLetter,
                ExplanationTitle = q.ExplanationTitle,
                Explanation = q.Explanation,
                TimeLimitSeconds = q.TimeLimitSeconds,
                IsActive = q.IsActive,
                CreatedAt = q.CreatedAt,
            })
            .ToListAsync();

        return new PagedPracticeQuestionsDto { Total = total, Page = page, PageSize = pageSize, Items = items };
    }

    public async Task<PracticeQuestionAdminResultDto> CreateQuestionAsync(SavePracticeQuestionDto dto)
    {
        var validated = await ValidateQuestionAsync(dto);

        var question = await unitOfWork.PracticeQuestions.InsertAsync(new PracticeQuestion
        {
            Subject = dto.Subject,
            Grade = dto.Grade,
            Difficulty = dto.Difficulty,
            Topic = validated.Topic,
            Text = validated.Text,
            OptionA = dto.OptionA,
            OptionB = dto.OptionB,
            OptionC = dto.OptionC,
            OptionD = dto.OptionD,
            CorrectLetter = validated.Letter,
            ExplanationTitle = NormalizeOptional(dto.ExplanationTitle),
            Explanation = dto.Explanation,
            TimeLimitSeconds = validated.TimeLimit,
            IsActive = dto.IsActive,
            CreatedBy = HttpContextHelper.UserId,
        });
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Practice question created: {subject}/{topic} #{id}",
            dto.Subject, validated.Topic, question.Id);
        return ToAdminDto(question);
    }

    public async Task<PracticeQuestionAdminResultDto> UpdateQuestionAsync(long id, SavePracticeQuestionDto dto)
    {
        var question = await unitOfWork.PracticeQuestions.SelectAsync(q => q.Id == id)
                       ?? throw new MilliyMockException(404, "Question not found");

        var validated = await ValidateQuestionAsync(dto);

        question.Subject = dto.Subject;
        question.Grade = dto.Grade;
        question.Difficulty = dto.Difficulty;
        question.Topic = validated.Topic;
        question.Text = validated.Text;
        question.OptionA = dto.OptionA;
        question.OptionB = dto.OptionB;
        question.OptionC = dto.OptionC;
        question.OptionD = dto.OptionD;
        question.CorrectLetter = validated.Letter;
        question.ExplanationTitle = NormalizeOptional(dto.ExplanationTitle);
        question.Explanation = dto.Explanation;
        question.TimeLimitSeconds = validated.TimeLimit;
        question.IsActive = dto.IsActive;
        question.UpdatedBy = HttpContextHelper.UserId;
        question.UpdatedAt = TimeHelper.GetDateTime();
        unitOfWork.PracticeQuestions.Update(question);
        await unitOfWork.SaveChangesAsync();

        return ToAdminDto(question);
    }

    public async Task<bool> DeleteQuestionAsync(long id)
    {
        var deleted = await unitOfWork.PracticeQuestions.DeleteAsync(q => q.Id == id);
        if (!deleted) throw new MilliyMockException(404, "Question not found");
        return await unitOfWork.SaveChangesAsync();
    }

    // ------------------------------------------------------------ Helpers

    private async Task<(string Topic, string Text, string Letter, int TimeLimit)> ValidateQuestionAsync(SavePracticeQuestionDto dto)
    {
        var text = dto.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            throw new MilliyMockException(400, "Text is required");

        if (string.IsNullOrWhiteSpace(dto.OptionA) || string.IsNullOrWhiteSpace(dto.OptionB) ||
            string.IsNullOrWhiteSpace(dto.OptionC) || string.IsNullOrWhiteSpace(dto.OptionD))
            throw new MilliyMockException(400, "All four options are required");

        var letter = dto.CorrectLetter?.Trim().ToUpperInvariant();
        if (letter is not ("A" or "B" or "C" or "D"))
            throw new MilliyMockException(400, "CorrectLetter must be one of A, B, C, D");

        if (dto.Grade is < 5 or > 11)
            throw new MilliyMockException(400, "Grade must be between 5 and 11, or null");

        // A missing/zero time falls back to 60s; otherwise clamp to a sane range.
        var timeLimit = dto.TimeLimitSeconds <= 0 ? 60 : dto.TimeLimitSeconds;
        if (timeLimit is < 5 or > 3600)
            throw new MilliyMockException(400, "TimeLimitSeconds must be between 5 and 3600");

        // The sync rule: a question may only live inside an existing topic
        // of the same subject.
        var topicSlug = NormalizeSlug(dto.Topic);
        var topic = await unitOfWork.PracticeTopics
            .SelectAsync(t => t.Subject == dto.Subject && t.Slug == topicSlug);
        if (topic is null)
            throw new MilliyMockException(400,
                $"Topic '{topicSlug}' does not exist for this subject. Create the topic first");

        return (topicSlug, text, letter, timeLimit);
    }

    private static string NormalizeSlug(string? slug)
    {
        slug = slug?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(slug) || !SlugRegex().IsMatch(slug))
            throw new MilliyMockException(400, "Slug must contain only lowercase letters, digits and dashes");
        return slug;
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PracticeTopicAdminResultDto ToAdminDto(PracticeTopic topic, int questionCount) => new()
    {
        Id = topic.Id,
        Subject = (int)topic.Subject,
        Slug = topic.Slug,
        Name = topic.Name,
        Section = topic.Section,
        Order = topic.Order,
        QuestionCount = questionCount,
    };

    private static PracticeQuestionAdminResultDto ToAdminDto(PracticeQuestion question) => new()
    {
        Id = question.Id,
        Subject = (int)question.Subject,
        Grade = question.Grade,
        Difficulty = (int)question.Difficulty,
        Topic = question.Topic,
        Text = question.Text,
        OptionA = question.OptionA,
        OptionB = question.OptionB,
        OptionC = question.OptionC,
        OptionD = question.OptionD,
        CorrectLetter = question.CorrectLetter,
        ExplanationTitle = question.ExplanationTitle,
        Explanation = question.Explanation,
        TimeLimitSeconds = question.TimeLimitSeconds,
        IsActive = question.IsActive,
        CreatedAt = question.CreatedAt,
    };
}
