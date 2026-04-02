using AutoMapper;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class OptionService(IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<OptionService> logger) : IOptionService
{
    public async Task<bool> CreateAsync(CreateOptionDto dto)
    {
        try
        {
            if ((dto.QuestionId == null && dto.QuestionGroupId == null) || (dto.QuestionId != null && dto.QuestionGroupId != null)) 
                throw new Exception("Option must belong to either Question OR QuestionGroup");

            logger.LogInformation("Creating option {text}", dto.Text);
            if (dto.QuestionId == null && dto.QuestionGroupId == null)
                throw new MilliyMockException(409, "Both ids cannot be null");

            var option = mapper.Map<Option>(dto);
            option.CreatedBy = HttpContextHelper.UserId;
            await unitOfWork.Options.InsertAsync(option);
            return await unitOfWork.Options.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating option {text}", dto.Text);
            throw new MilliyMockException();
        }
    }

    public async Task<bool> UpdateAsync(long optionId, UpdateOptionDto dto)
    {
        try
        {
            logger.LogInformation("Updating option with id {optionId}", optionId);
            var option = await unitOfWork.Options.SelectAsync(o => o.Id == optionId);
            if (option == null)
                throw new MilliyMockException(404, "Option not found");

            mapper.Map(dto, option);
            option.UpdatedBy = HttpContextHelper.UserId;
            option.UpdatedAt = TimeHelper.GetDateTime();
            unitOfWork.Options.Update(option);
            return await unitOfWork.Options.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating option with id {optionId}", optionId);
            throw new MilliyMockException();
        }
    }

    public async Task<bool> DeleteAsync(long optionId)
    {
        try
        {
            logger.LogInformation("Deleting option with id {optionId}", optionId);
            var option = await unitOfWork.Options.SelectAsync(o => o.Id == optionId);
            if (option == null)
                throw new MilliyMockException(404, "Option not found");

            await unitOfWork.Options.DeleteAsync(o => o.Id == optionId);
            return await unitOfWork.Options.SaveAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting option with id {optionId}", optionId);
            throw new MilliyMockException();
        }
    }
}