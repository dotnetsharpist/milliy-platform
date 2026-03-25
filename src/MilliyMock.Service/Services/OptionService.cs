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
            logger.LogInformation("Creating option {text}", dto.Text);
            if (dto.QuestionId == null && dto.QuestionGroupId == null)
                throw new MilliyMockException(409, "Both ids cannot be null");

            var option = mapper.Map<Option>(dto);
            option.CreatedBy = HttpContextHelper.UserId;
            await unitOfWork.Options.InsertAsync(option);
            return await unitOfWork.Options.SaveAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating option {text}", dto.Text);
            throw new MilliyMockException();
        }
    }
}