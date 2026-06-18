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
                return "Usage: /addbalance <@username | telegramId | email> <amount>";

            var userIdentifier = parts[1];
            if (!int.TryParse(parts[2], out var amount))
                return "Amount must be a whole number.";

            User? user = null;
            if (long.TryParse(userIdentifier, out var telegramUserId))
            {
                user = await unitOfWork.Users.SelectAsync(u => u.BotUser != null && u.BotUser.TgUserId == telegramUserId);
                if (user is null) return "User with this telegram user id doesn't exist";
            }
            else if (userIdentifier.StartsWith('@'))
            {
                var handle = userIdentifier.TrimStart('@');
                var botUser = await unitOfWork.BotUsers.SelectAsync(bu => bu.Username == handle);
                if (botUser is null) return "User with this telegram username doesn't exist";

                user = await unitOfWork.Users.SelectAsync(u => u.BotUserId == botUser.Id);
                if (user is null) return "This telegram user hasn't linked an account yet";
            }
            else if (userIdentifier.Contains('@'))
            {
                user = await unitOfWork.Users.SelectAsync(u => u.Email == userIdentifier);
                if (user is null) return "User with this email doesn't exist";
            }
            else return "Couldn't recognize the user. Use @username, telegram id, or email.";

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