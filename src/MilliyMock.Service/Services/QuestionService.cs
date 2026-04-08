using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Questions;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class QuestionService(
    IUnitOfWork unitOfWork, 
    IFileService fileService,
    IMapper mapper,
    ILogger<QuestionService> logger) : IQuestionService
{
    public async Task<bool> CreateAsync(CreateQuestionDto dto)
    {
        try
        {
            var question = mapper.Map<Question>(dto);
            question.CreatedBy = HttpContextHelper.UserId;

            if (dto.QuestionGroupId is not null)
            {
                var questionGroup = await unitOfWork.QuestionGroups.SelectAsync(qg => qg.Id == dto.QuestionGroupId);
                if (questionGroup is null) throw new MilliyMockException(404, "Question group not found");
                question.TestId = questionGroup.TestId;
            }
            
            else
            {
                var test = await unitOfWork.Tests.SelectAsync(t => t.Id == dto.TestId);
                if (test is null) throw new MilliyMockException(404, "Test not found");
            }

            if (dto.TextUz is not null && dto.TextRu is not null)
            {
                string? imagePathUz = null;
                string? imagePathRu = null;
                
                if (dto.ImageUz is not null && dto.ImageRu is not null)
                {
                    imagePathUz = await fileService.UploadImage(dto.ImageUz);
                    imagePathRu = await fileService.UploadImage(dto.ImageRu);
                }

                var questionTranslationUz = new Translation()
                {
                    Language = Language.Uzbek,
                    Text = dto.TextUz,
                    ImagePath = imagePathUz,
                    Question = question
                };
                var questionTranslationRu = new Translation()
                {
                    Language = Language.Russian,
                    Text = dto.TextRu,
                    ImagePath = imagePathRu,
                    Question = question
                };
                question.Translations = new List<Translation>() { questionTranslationUz, questionTranslationRu };
            }
            
            if (dto.Options is not null && dto.Options.Count > 0)
            {
                question.Options = mapper.Map<List<Option>>(dto.Options);
                foreach (var option in question.Options)
                    option.CreatedBy = HttpContextHelper.UserId;
            }

            await unitOfWork.Questions.InsertAsync(question);
            return await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating question {text}", dto.TextUz);
            throw new MilliyMockException();
        }
    }

    public async Task<bool> UpdateAsync(long questionId, UpdateQuestionDto dto)
    {
        try
        {
            logger.LogInformation("Updating question with id {questionId}", questionId);

            var question = await unitOfWork.Questions
                .SelectAll(q => q.Id == questionId && !q.IsDeleted)
                .Include(q => q.Translations)
                .Include(q => q.Options)
                .FirstOrDefaultAsync();

            if (question is null) throw new MilliyMockException(404, "Question not found");

            // --- update translations ---
            await UpdateTranslationAsync(question, Language.Uzbek, dto.TextUz, dto.ImageUz);
            await UpdateTranslationAsync(question, Language.Russian, dto.TextRu, dto.ImageRu);

            // --- update options ---
            if (dto.Options is not null)
            {
                var keepIds = dto.Options
                    .Where(o => o.Id.HasValue)
                    .Select(o => o.Id!.Value)
                    .ToHashSet();

                // remove options that were not sent back
                var toRemove = question.Options.Where(o => !keepIds.Contains(o.Id)).ToList();
                foreach (var opt in toRemove)
                    question.Options.Remove(opt);

                foreach (var optDto in dto.Options)
                {
                    if (optDto.Id.HasValue)
                    {
                        // update existing
                        var existing = question.Options.FirstOrDefault(o => o.Id == optDto.Id.Value);
                        if (existing is not null)
                        {
                            existing.Text = optDto.Text;
                            existing.IsCorrect = optDto.IsCorrect;
                            existing.UpdatedBy = HttpContextHelper.UserId;
                            existing.UpdatedAt = TimeHelper.GetDateTime();
                        }
                    }
                    else
                    {
                        // add new
                        question.Options.Add(new Option
                        {
                            Text = optDto.Text,
                            IsCorrect = optDto.IsCorrect,
                            CreatedBy = HttpContextHelper.UserId
                        });
                    }
                }
            }

            mapper.Map(dto, question);
            question.UpdatedBy = HttpContextHelper.UserId;
            question.UpdatedAt = TimeHelper.GetDateTime();

            unitOfWork.Questions.Update(question);
            return await unitOfWork.SaveChangesAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating question with id {questionId}", questionId);
            throw new MilliyMockException();
        }
    }

    private async Task UpdateTranslationAsync(Question question, Language language, string? newText, IFormFile? newImage)
    {
        if (newText is null) return;

        var translation = question.Translations.FirstOrDefault(t => t.Language == language);
        if (translation is null)
        {
            translation = new Translation { Language = language, Question = question, Text = newText };
            question.Translations.Add(translation);
        }
        else
        {
            translation.Text = newText;
        }

        if (newImage is not null)
        {
            if (translation.ImagePath is not null)
                fileService.Delete(translation.ImagePath);
            translation.ImagePath = await fileService.UploadImage(newImage);
        }
    }

    public async Task<List<QuestionResultDto>> GetByTestIdAsync(long testId)
    {
        var questions = await unitOfWork.Questions
            .SelectAll(q => q.TestId == testId && !q.IsDeleted)
            .Include(q => q.Translations)
            .Include(q => q.Options)
            .ToListAsync();
        return mapper.Map<List<QuestionResultDto>>(questions);
    }

    public async Task<bool> DeleteAsync(long questionId)
    {
        try
        {
            logger.LogInformation("Deleting question with id {questionId}", questionId);
            var question = await unitOfWork.Questions
                .SelectAll(q => q.Id == questionId)
                .Include(q => q.Translations)
                .FirstOrDefaultAsync();
            if (question is null) throw new MilliyMockException(404, "Question not found");
            
            // delete translation images if they exist
            foreach (var translation in question.Translations)
            {
                if (translation.ImagePath is not null)
                {
                    var deleted = fileService.Delete(translation.ImagePath);
                    if (!deleted)
                        logger.LogError("Failed to delete image at path {imagePath} for question {questionId}",
                            translation.ImagePath, questionId);
                }
            }

            await unitOfWork.Questions.DeleteAsync(q => q.Id == questionId);
            return await unitOfWork.Questions.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting question with id {questionId}", questionId);
            throw new MilliyMockException();
        }
    }
}