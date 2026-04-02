using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
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
            var test = await unitOfWork.Tests.SelectAsync(t => t.Id == dto.TestId);
            if (test is null) throw new MilliyMockException(404, "Test not found");

            if (dto.QuestionGroupId is not null)
            {
                var questionGroup = await unitOfWork.QuestionGroups.SelectAsync(qg => qg.Id == dto.QuestionGroupId);
                if (questionGroup is null) throw new MilliyMockException(404, "Question group not found");
            }

            var question = mapper.Map<Question>(dto);
            question.CreatedBy = HttpContextHelper.UserId;
            if (dto.Image is not null)
            {
                var imagePath = await fileService.UploadImage(dto.Image);
                question.ImagePath = imagePath;
            }

            await unitOfWork.Questions.InsertAsync(question);
            return await unitOfWork.Questions.SaveAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating question {text}", dto.Text);
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
                .Include(q => q.Options)
                .FirstOrDefaultAsync();

            if (question is null) throw new MilliyMockException(404, "Question not found");

            // image replacement
            if (dto.Image is not null)
            {
                if (question.ImagePath is not null)
                    fileService.Delete(question.ImagePath);
                question.ImagePath = await fileService.UploadImage(dto.Image);
            }

            mapper.Map(dto, question);
            question.UpdatedBy = HttpContextHelper.UserId;
            question.UpdatedAt = TimeHelper.GetDateTime();

            // Handle options if provided
            if (dto.Options is not null)
            {
                var existingOptions = question.Options.Where(o => !o.IsDeleted).ToList();
                var incomingIds = dto.Options.Where(o => o.Id > 0).Select(o => o.Id).ToHashSet();

                // Soft-delete options not present in the incoming list
                /*
                foreach (var option in existingOptions)
                {
                    if (!incomingIds.Contains(option.Id)) 
                        await unitOfWork.Options.DeleteAsync(o => o.Id == option.Id);
                }
                */

                foreach (var optionDto in dto.Options)
                {
                    if (optionDto.Id > 0)
                    {
                        // Update existing option
                        var existing = existingOptions.FirstOrDefault(o => o.Id == optionDto.Id);
                        if (existing is not null)
                        {
                            existing.Text = optionDto.Text;
                            existing.IsCorrect = optionDto.IsCorrect;
                            existing.UpdatedBy = HttpContextHelper.UserId;
                            existing.UpdatedAt = TimeHelper.GetDateTime();
                        }
                    }
                    else
                    {
                        await unitOfWork.Options.InsertAsync(new Option
                        {
                            Text = optionDto.Text,
                            IsCorrect = optionDto.IsCorrect,
                            QuestionId = questionId,
                            CreatedBy = HttpContextHelper.UserId
                        });
                    }
                }
            }

            unitOfWork.Questions.Update(question);
            return await unitOfWork.Questions.SaveAsync();
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

    public async Task<List<QuestionResultDto>> GetByTestIdAsync(long testId)
    {
        var questions = await unitOfWork.Questions.SelectAll(q => q.TestId == testId).ToListAsync();
        return mapper.Map<List<QuestionResultDto>>(questions);
    }

    public async Task<bool> DeleteAsync(long questionId)
    {
        try
        {
            logger.LogInformation("Deleting question with id {questionId}", questionId);
            var question = await unitOfWork.Questions.SelectAsync(q => q.Id == questionId);
            if (question is null) throw new MilliyMockException(404, "Question not found");
            
            // delete image if exists
            if (question.ImagePath is not null)
            {
                var deleteImage = fileService.Delete(question.ImagePath);
                if (deleteImage is false)
                {
                    logger.LogError("Failed to delete image at path {imagePath} for question {questionId}",
                        question.ImagePath, questionId);
                    //throw new MilliyMockException(500, "Failed to delete question image");
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