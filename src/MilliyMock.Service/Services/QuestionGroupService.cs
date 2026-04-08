using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.QuestionGroups;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class QuestionGroupService(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IMapper mapper,
    ILogger<QuestionGroupService> logger) : IQuestionGroupService
{
    public async Task<bool> CreateAsync(CreateQuestionGroupDto dto)
    {
        try
        {
            var questionGroup = new QuestionGroup
            {
                TestId = dto.TestId,
                CreatedBy = HttpContextHelper.UserId
            };

            // verify test exists
            var test = await unitOfWork.Tests.SelectAsync(t => t.Id == dto.TestId);
            if (test is null) throw new MilliyMockException(404, "Test not found");

            // build translations
            if (dto.TitleUz is not null || dto.TitleRu is not null)
            {
                if (dto.TitleUz is not null)
                {
                    string? imagePathUz = dto.ImageUz is not null
                        ? await fileService.UploadImage(dto.ImageUz)
                        : null;

                    questionGroup.Translations.Add(new Translation
                    {
                        Language = Language.Uzbek,
                        Text = dto.TitleUz,
                        ImagePath = imagePathUz,
                        QuestionGroup = questionGroup
                    });
                }

                if (dto.TitleRu is not null)
                {
                    string? imagePathRu = dto.ImageRu is not null
                        ? await fileService.UploadImage(dto.ImageRu)
                        : null;

                    questionGroup.Translations.Add(new Translation
                    {
                        Language = Language.Russian,
                        Text = dto.TitleRu,
                        ImagePath = imagePathRu,
                        QuestionGroup = questionGroup
                    });
                }
            }

            await unitOfWork.QuestionGroups.InsertAsync(questionGroup);
            return await unitOfWork.QuestionGroups.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating QuestionGroup {text}", dto.TitleUz);
            throw new MilliyMockException();
        }
    }

    public async Task<bool> UpdateAsync(long questionGroupId, UpdateQuestionGroupDto dto)
    {
        try
        {
            logger.LogInformation("Updating question group with id {questionGroupId}", questionGroupId);

            var questionGroup = await unitOfWork.QuestionGroups
                .SelectAll(qg => qg.Id == questionGroupId && !qg.IsDeleted)
                .Include(qg => qg.Translations)
                .FirstOrDefaultAsync();

            if (questionGroup is null) throw new MilliyMockException(404, "Question group not found");

            await UpdateTranslationAsync(questionGroup, Language.Uzbek, dto.TitleUz, dto.ImageUz);
            await UpdateTranslationAsync(questionGroup, Language.Russian, dto.TitleRu, dto.ImageRu);

            questionGroup.UpdatedBy = HttpContextHelper.UserId;
            questionGroup.UpdatedAt = TimeHelper.GetDateTime();

            unitOfWork.QuestionGroups.Update(questionGroup);
            return await unitOfWork.QuestionGroups.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating question group with id {questionGroupId}", questionGroupId);
            throw new MilliyMockException();
        }
    }

    public async Task<QuestionGroupResultDto> GetByIdAsync(long questionGroupId)
    {
        var questionGroup = await unitOfWork.QuestionGroups
            .SelectAll(qg => qg.Id == questionGroupId && !qg.IsDeleted)
            .Include(qg => qg.Translations)
            .Include(qg => qg.Questions).ThenInclude(q => q.Translations)
            .Include(qg => qg.Questions).ThenInclude(q => q.Options)
            .Include(qg => qg.Options)
            .FirstOrDefaultAsync();

        if (questionGroup is null) throw new MilliyMockException(404, "Question group not found");

        return mapper.Map<QuestionGroupResultDto>(questionGroup);
    }

    public async Task<List<QuestionGroupResultDto>> GetByTestIdAsync(long testId)
    {
        var questionGroups = await unitOfWork.QuestionGroups
            .SelectAll(qg => qg.TestId == testId && !qg.IsDeleted)
            .Include(qg => qg.Translations)
            .Include(qg => qg.Questions).ThenInclude(q => q.Translations)
            .Include(qg => qg.Questions).ThenInclude(q => q.Options)
            .Include(qg => qg.Options)
            .ToListAsync();

        return mapper.Map<List<QuestionGroupResultDto>>(questionGroups);
    }

    public async Task<bool> DeleteAsync(long questionGroupId)
    {
        try
        {
            logger.LogInformation("Deleting question group with id {questionGroupId}", questionGroupId);

            var questionGroup = await unitOfWork.QuestionGroups
                .SelectAll(qg => qg.Id == questionGroupId)
                .Include(qg => qg.Translations)
                .FirstOrDefaultAsync();

            if (questionGroup is null) throw new MilliyMockException(404, "Question group not found");

            // delete translation images
            foreach (var translation in questionGroup.Translations)
            {
                if (translation.ImagePath is not null)
                {
                    var deleted = fileService.Delete(translation.ImagePath);
                    if (!deleted)
                        logger.LogError("Failed to delete image at path {imagePath}", translation.ImagePath);
                }
            }

            await unitOfWork.QuestionGroups.DeleteAsync(qg => qg.Id == questionGroupId);
            return await unitOfWork.QuestionGroups.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting question group with id {questionGroupId}", questionGroupId);
            throw new MilliyMockException();
        }
    }

    private async Task UpdateTranslationAsync(QuestionGroup group, Language language, string? newTitle, IFormFile? newImage)
    {
        if (newTitle is null) return;

        var translation = group.Translations.FirstOrDefault(t => t.Language == language);
        if (translation is null)
        {
            translation = new Translation { Language = language, QuestionGroup = group, Text = newTitle };
            group.Translations.Add(translation);
        }
        else
        {
            translation.Text = newTitle;
        }

        if (newImage is not null)
        {
            if (translation.ImagePath is not null)
                fileService.Delete(translation.ImagePath);
            translation.ImagePath = await fileService.UploadImage(newImage);
        }
    }
}