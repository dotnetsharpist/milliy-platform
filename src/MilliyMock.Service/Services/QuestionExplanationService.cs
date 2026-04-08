using AutoMapper;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.QuestionExplanations;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class QuestionExplanationService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<QuestionExplanationService> logger) : IQuestionExplanationService
{
    public async Task<bool> CreateAsync(CreateQuestionExplanationDto dto)
    {
        try
        {
            logger.LogInformation("Creating question explanation for question with id {QuestionId}", dto.QuestionId);
            var question = await unitOfWork.Questions.SelectAsync(q => q.Id == dto.QuestionId);
            if (question is null) throw new MilliyMockException(404, "Question not found");

            var questionExplanation = mapper.Map<QuestionExplanation>(dto);
            questionExplanation.CreatedBy = HttpContextHelper.UserId;

            await unitOfWork.QuestionExplanations.InsertAsync(questionExplanation);
            return await unitOfWork.SaveChangesAsync();
        }

        catch (MilliyMockException ex)
        {
            logger.LogError(ex,
                "Error occurred while creating question explanation for question with id {QuestionId}: {Message}",
                dto.QuestionId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error occurred while creating question explanation for question with id {QuestionId}: {Message}",
                dto.QuestionId, ex.Message);
            throw new MilliyMockException();
        }
    }

    public async Task<bool> UpdateAsync(long questionExplanationId, UpdateQuestionExplanationDto dto)
    {
        try
        {
            logger.LogInformation("Updating question explanation with id {QuestionExplanationId}", questionExplanationId);
            var questionExplanation = await unitOfWork.QuestionExplanations.SelectAsync(qe => qe.Id == questionExplanationId);
            if (questionExplanation is null) throw new MilliyMockException(404, "Question explanation not found");

            mapper.Map(dto, questionExplanation);
            questionExplanation.UpdatedBy = HttpContextHelper.UserId;

            unitOfWork.QuestionExplanations.Update(questionExplanation);
            return await unitOfWork.SaveChangesAsync();
        }

        catch (MilliyMockException ex)
        {
            logger.LogError(ex,
                "Error occurred while updating question explanation with id {QuestionExplanationId}: {Message}",
                questionExplanationId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error occurred while updating question explanation with id {QuestionExplanationId}: {Message}",
                questionExplanationId, ex.Message);
            throw new MilliyMockException();
        };
    }

    public async Task<QuestionExplanationResultDto> GetByQuestionIdAsync(long questionId)
    {
        try
        {
            logger.LogInformation("Getting question explanation for question with id {QuestionId}", questionId);
            var questionExplanation = await unitOfWork.QuestionExplanations.SelectAsync(qe => qe.QuestionId == questionId);
            if (questionExplanation is null) throw new MilliyMockException(404, "Question explanation not found");

            return mapper.Map<QuestionExplanationResultDto>(questionExplanation);
        }
        catch (MilliyMockException ex)
        {
            logger.LogError(ex,
                "Error occurred while getting question explanation for question with id {QuestionId}: {Message}",
                questionId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error occurred while getting question explanation for question with id {QuestionId}: {Message}",
                questionId, ex.Message);
            throw new MilliyMockException();
        }
    }

    public async Task<bool> DeleteAsync(long questionExplanationId)
    {
        try
        {
            logger.LogInformation("Deleting question explanation with id {QuestionExplanationId}", questionExplanationId);
            var questionExplanation = await unitOfWork.QuestionExplanations.SelectAsync(qe => qe.Id == questionExplanationId);
            if (questionExplanation is null) throw new MilliyMockException(404, "Question explanation not found");

            await unitOfWork.QuestionExplanations.DeleteAsync(qe => qe.Id == questionExplanationId);
            return await unitOfWork.SaveChangesAsync();
        }
        catch (MilliyMockException ex)
        {
            logger.LogError(ex,
                "Error occurred while deleting question explanation with id {QuestionExplanationId}: {Message}",
                questionExplanationId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error occurred while deleting question explanation with id {QuestionExplanationId}: {Message}",
                questionExplanationId, ex.Message);
            throw new MilliyMockException();
        }
    }
}