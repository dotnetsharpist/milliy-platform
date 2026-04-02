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