using Microsoft.Extensions.Logging;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class TransactionService(
    IUnitOfWork unitOfWork,
    ILogger<TransactionService> logger) : ITransactionService
{
    public async Task<bool> ApplyTransactionAsync(long userId, int amount, BalanceTransactionType type, string? description, long? testId)
    {
        try
        {
            logger.LogInformation("Applying transaction for user {userId}", userId);
            var balance = await unitOfWork.UserBalances.SelectAsync(b => b.UserId == userId);
            if (balance is null) throw new MilliyMockException();

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

            return await unitOfWork.TransactionHistories.SaveAsync();

        }
        catch (MilliyMockException)
        {
            throw;
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying transaction for user {userId}", userId);
            throw new MilliyMockException();
        }
    }
}