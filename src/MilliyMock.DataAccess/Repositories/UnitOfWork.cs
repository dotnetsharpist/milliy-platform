using MilliyMock.DataAccess.Contexts;
using MilliyMock.DataAccess.IRepositories;
using MilliyMock.Domain.Entities;

namespace MilliyMock.DataAccess.Repositories;

public class UnitOfWork(MilliyMockDbContext dbContext) : IUnitOfWork
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IRepository<User> Users { get; } = new Repository<User>(dbContext);
    
    public async Task<bool> SaveChangesAsync()
    {
        return await dbContext.SaveChangesAsync() >= 0;
    }
}