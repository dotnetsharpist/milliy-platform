using AutoMapper;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Service.Dtos.Options;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class OptionService(IUnitOfWork unitOfWork, IMapper mapper) : IOptionService
{
    public async Task<bool> CreateAsync(CreateOptionDto dto)
    {
        var option = mapper.Map<Option>(dto);
        option.CreatedBy = HttpContextHelper.UserId;
        await unitOfWork.Options.InsertAsync(option);
        return await unitOfWork.Options.SaveAsync();
    }
}