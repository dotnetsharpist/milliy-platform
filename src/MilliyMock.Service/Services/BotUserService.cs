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

            var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return "Usage: /addbalance <user id> <amount>";

            var userIdentifier = parts[1];
            if (!int.TryParse(parts[2], out var amount))
                return "Amount must be a whole number.";

            User? user = null;
            if (long.TryParse(userIdentifier, out var userId))
            {
                user = await unitOfWork.Users.SelectAsync(u=> u.Id == userId);
                if (user is null) return "User with this telegram user id doesn't exist";
            }

            else return "nigga that's not an id";

            var adjustDto = new AdjustBalanceDto
            {
                Amount = amount,
                UserId = user.Id
            };
            await userBalanceService.AdjustAsync(adjustDto);
            return $"Done. Adjusted balance by {amount}.";
        }
        catch (MilliyMockException ex)
        {
            logger.LogWarning(ex, "Add balance via bot rejected: {text}", messageText);
            return ex.Message;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding balance with messageText: {text}", messageText);
            return "Something went wrong while adjusting the balance.";
        }
    }

}