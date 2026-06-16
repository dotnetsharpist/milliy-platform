using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.BotUsers;
using MilliyMock.Service.Dtos.UserBalances;
using MilliyMock.Service.Interfaces;

namespace MilliyMock.Service.Services;

public class BotUserService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserBalanceService userBalanceService,
    ILogger<BotUserService> logger) : IBotUserService
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
    
    public async Task<string> AddBalanceViaBotAsync(string messageText)
    {
        try
        {
            logger.LogInformation("Adding balance via bot with messageText: {text}", messageText);
            var userIdentifier = messageText.Split(' ')[1];
            var amountP = messageText.Split(' ')[2];
            if (!int.TryParse(amountP, out var amount))
                return "I don't understand";
            
            User? user = null;
            if (long.TryParse(userIdentifier, out var userId))
            {
                user = await unitOfWork.Users.SelectAsync(u => u.BotUserId == userId);
                if (user is null) return "User with this telegram user id doesn't exist";
            }

            else if (userIdentifier.StartsWith('@'))
            {
                var botUser = await unitOfWork.BotUsers.SelectAsync(bu => bu.Username == userIdentifier);
                if (botUser is null) return "User with this telegram username doesn't exist";
            }
            else if (userIdentifier.Contains('@'))
            {
                user = await unitOfWork.Users.SelectAsync(u => u.Email == userIdentifier);
                if (user is null) return "User with this email doesn't exist";
            }
            else return "I don't understand.";

            var adjustDto = new AdjustBalanceDto
            {
                Amount = amount,
                UserId = user!.Id
            };
            await userBalanceService.AdjustAsync(adjustDto);
            return "Done";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating adding balance with messageText: {text}", messageText);
            throw new MilliyMockException();
        }
    }

}