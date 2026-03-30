using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
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
            var questionGroup = mapper.Map<QuestionGroup>(dto);
            questionGroup.CreatedBy = HttpContextHelper.UserId;
            
            if (dto.Image is not null)
            {
                var imagePath = await fileService.UploadImage(dto.Image);
                questionGroup.ImagePath = imagePath;
            }

            await unitOfWork.QuestionGroups.InsertAsync(questionGroup);
            return await unitOfWork.QuestionGroups.SaveAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating QuestionGroup {text}", dto.Title);
            throw new MilliyMockException();
        }
    }

    public async Task<List<QuestionGroupResultDto>> GetByTestIdAsync(long testId)
    {
        var questionGroups = await unitOfWork.QuestionGroups
            .SelectAll(qg => qg.TestId == testId)
            .Include(g => g.Questions)
            .Include(g => g.Options)
            .ToListAsync();
        
        return mapper.Map<List<QuestionGroupResultDto>>(questionGroups);
    }

    public async Task<bool> DeleteAsync(long questionGroupId)
    {
        try
        {
            logger.LogInformation("Deleting question group with id {questionGroupId}", questionGroupId);
            var questionGroup = await unitOfWork.QuestionGroups.SelectAsync(qg => qg.Id == questionGroupId);
            if (questionGroup == null) throw new MilliyMockException(404, "Question group not found");
            
            // delete image if exists
            if (!string.IsNullOrEmpty(questionGroup.ImagePath))
            { 
                var deleteImage = fileService.Delete(questionGroup.ImagePath);
                if (!deleteImage)
                {
                    logger.LogError("Failed to delete image at path {imagePath}", questionGroup.ImagePath);
                    //throw new MilliyMockException(500, "Failed to delete associated image");
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
}