using AutoMapper;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class OptionService(IUnitOfWork unitOfWork, IMapper mapper) : IOptionService
{
    public async Task<bool> CreateAsync(CreateOptionDto dto)
    {
        if (dto.QuestionId == null && dto.QuestionGroupId == null)
            throw new MilliyMockException(409, "Both ids cannot be null");
        
        var option = mapper.Map<Option>(dto);
        option.CreatedBy = HttpContextHelper.UserId;
        await unitOfWork.Options.InsertAsync(option);
        return await unitOfWork.Options.SaveAsync();
    }
}