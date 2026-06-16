using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Dtos.TransactionHistories;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class TransactionHistoryService(
    IUnitOfWork unitOfWork,
    IMapper mapper) : ITransactionHistoryService
{
    public async Task<List<TransactionHistoryResultDto>> GetMyTransactionsAsync()
    {
        var userId = HttpContextHelper.UserId ?? throw new MilliyMockException(409, "Unauthorized");
        return await GetByUserIdAsync(userId);
    }

    public async Task<List<TransactionHistoryResultDto>> GetByUserIdAsync(long userId)
    {
        var transactions = await unitOfWork.TransactionHistories
            .SelectAll(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return mapper.Map<List<TransactionHistoryResultDto>>(transactions);
    }
}
