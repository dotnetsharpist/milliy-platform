using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.BotUsers;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Service.Services;

public class BotUserService(IUnitOfWork unitOfWork, IMapper mapper) : IBotUserService
{
    public async ValueTask<bool> CreateAsync(CreateBotUserDto dto)
    {
        var exists = await unitOfWork.BotUsers
            .SelectAsync(bu => bu.TgUserId == dto.TgUserId);

        if (exists is not null)
            return true;
        
        var mappedBotUser = mapper.Map<BotUser>(dto);
        await unitOfWork.BotUsers.InsertAsync(mappedBotUser);
        return await unitOfWork.BotUsers.SaveAsync();
    }

    public async ValueTask<List<BotUserResultDto>> GetAllAsync()
    {
        var botUsers = await unitOfWork.BotUsers
            .SelectAll(bu => !bu.IsDeleted)
            .ToListAsync();

        return mapper.Map<List<BotUserResultDto>>(botUsers);
    }

    public async ValueTask<BotUserResultDto> GeByIdAsync(long id)
    {
        var botUser = await unitOfWork.BotUsers.SelectAsync(bu => bu.TgUserId == id);
        if (botUser is null) throw new MilliyMockException(404, "User not found");

        return mapper.Map<BotUserResultDto>(botUser);
    }
}