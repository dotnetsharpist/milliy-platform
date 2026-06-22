using AutoMapper;
using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.UserBalances;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class UserBalanceService(
    IUnitOfWork unitOfWork,
    ITransactionService transactionService,
    IMapper mapper,
    ILogger<UserBalanceService> logger) : IUserBalanceService
{
    public async Task<UserBalanceResultDto> GetMyBalanceAsync()
    {
        var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");
        var balance = await GetOrCreateBalanceAsync(userId);
        return mapper.Map<UserBalanceResultDto>(balance);
    }

    public async Task<UserBalanceResultDto> GetByUserIdAsync(long userId)
    {
        var balance = await GetOrCreateBalanceAsync(userId);
        return mapper.Map<UserBalanceResultDto>(balance);
    }
    
    public async Task<bool> AdjustAsync(AdjustBalanceDto dto)
    {
        if (dto.Amount == 0) throw new MilliyMockException(400, "Amount must not be zero");

        var user = await unitOfWork.Users.SelectAsync(u => u.Id == dto.UserId);
        if (user is null) throw new MilliyMockException(404, "User not found");

        var applyTransaction = await transactionService.ApplyTransactionAsync(dto.UserId, dto.Amount, BalanceTransactionType.AdminAdjustment, dto.Description, null);
        if (!applyTransaction) throw new MilliyMockException();
        return await unitOfWork.SaveChangesAsync();
    }
    
    private async Task<UserBalance> GetOrCreateBalanceAsync(long userId)
    {
        var balance = await unitOfWork.UserBalances.SelectAsync(b => b.UserId == userId);
        if (balance is not null) return balance;

        balance = new UserBalance
        {
            UserId = userId,
            Balance = 0,
            CreatedBy = HttpContextHelper.UserId
        };

        await unitOfWork.UserBalances.InsertAsync(balance);
        await unitOfWork.SaveChangesAsync();

        return balance;
    }
}
