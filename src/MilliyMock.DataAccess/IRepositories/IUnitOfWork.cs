using MilliyMock.Domain.Entities;

namespace MilliyMock.DataAccess.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<BotUser> BotUsers { get; }
    IRepository<Test> Tests { get; }
    IRepository<QuestionGroup> QuestionGroups { get; }
    IRepository<Question> Questions { get; }
    IRepository<Translation> Translations { get; }
    IRepository<QuestionExplanation> QuestionExplanations { get; }
    IRepository<Option> Options { get; }
    IRepository<UserTestAttempt> UserTestAttempts { get; }
    IRepository<UserAnswer> UserAnswer { get; }
    Task<bool> SaveChangesAsync();
}