using MilliyMock.Domain.Entities;

namespace MilliyMock.DataAccess.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    Task<bool> SaveChangesAsync();
}