using AutoMapper;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.TempUsers;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Service.Services;

public class TempUserService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<TempUserService> logger) : ITempUserService
{
    public async Task<TempUserResultDto> CreateAsync(CreateTempUserDto dto)
    {
        try
        {
            logger.LogInformation("Creating temp user with first name: {FirstName}, last name: {LastName}, father name: {FatherName}",
                dto.FirstName, dto.LastName, dto.FatherName);

            var tempUser = mapper.Map<TempUser>(dto);
            await unitOfWork.TempUsers.InsertAsync(tempUser);
            await unitOfWork.TempUsers.SaveAsync();
            return mapper.Map<TempUserResultDto>(tempUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating temp user");
            throw new MilliyMockException();
        }
    }
}