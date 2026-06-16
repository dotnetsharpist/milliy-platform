using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        await ApplyTransactionAsync(dto.UserId, dto.Amount, BalanceTransactionType.AdminAdjustment, dto.Description, null);
        return await unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> PurchaseTestAsync(long testId)
    {
        try
        {
            logger.LogInformation("Purchasing test {testId} for user {userId}", testId, HttpContextHelper.UserId);
            var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");

            var test = await unitOfWork.Tests.SelectAsync(t => t.Id == testId);
            if (test is null) throw new MilliyMockException(404, "Test not found");

            if (!test.IsPremium || test.Price <= 0)
                throw new MilliyMockException(400, "Test is not premium");

            var alreadyPurchased = await unitOfWork.TransactionHistories
                .SelectAll(t => t.UserId == userId
                                && t.TestId == testId
                                && t.Type == BalanceTransactionType.Purchase)
                .AnyAsync();

            if (alreadyPurchased)
                throw new MilliyMockException(409, "Test already purchased");

            await ApplyTransactionAsync(userId, -test.Price, BalanceTransactionType.Purchase,
                $"Purchase of test '{test.Title}'", testId);

            return await unitOfWork.SaveChangesAsync();
        }
        catch (MilliyMockException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error purchasing test {testId}", testId);
            throw new MilliyMockException();
        }
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

    private async Task ApplyTransactionAsync(
        long userId,
        int amount,
        BalanceTransactionType type,
        string? description,
        long? testId)
    {
        var balance = await GetOrCreateBalanceAsync(userId);

        var balanceBefore = balance.Balance;
        var balanceAfter = balanceBefore + amount;

        if (balanceAfter < 0)
            throw new MilliyMockException(400, "Insufficient balance");

        balance.Balance = balanceAfter;
        balance.UpdatedBy = HttpContextHelper.UserId;
        balance.UpdatedAt = TimeHelper.GetDateTime();
        unitOfWork.UserBalances.Update(balance);

        await unitOfWork.TransactionHistories.InsertAsync(new TransactionHistory
        {
            UserId = userId,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Type = type,
            Description = description,
            TestId = testId,
            CreatedBy = HttpContextHelper.UserId
        });
    }
}
