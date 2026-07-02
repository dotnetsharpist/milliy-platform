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
    public IRepository<TempUser> TempUsers { get; } = new Repository<TempUser>(dbContext);
    public IRepository<BotUser> BotUsers { get; } = new Repository<BotUser>(dbContext);
    public IRepository<Test> Tests { get; } = new Repository<Test>(dbContext);
    public IRepository<QuestionGroup> QuestionGroups { get; } = new Repository<QuestionGroup>(dbContext);
    public IRepository<Question> Questions { get; } = new Repository<Question>(dbContext);
    public IRepository<Translation> Translations { get; } = new Repository<Translation>(dbContext);
    public IRepository<QuestionExplanation> QuestionExplanations { get; } = new Repository<QuestionExplanation>(dbContext);
    public IRepository<Option> Options { get; } = new Repository<Option>(dbContext);
    public IRepository<UserTestAttempt> UserTestAttempts { get; } = new Repository<UserTestAttempt>(dbContext);
    public IRepository<UserAnswer> UserAnswer { get; } = new Repository<UserAnswer>(dbContext);
    public IRepository<UserBalance> UserBalances { get; } = new Repository<UserBalance>(dbContext);
    public IRepository<TransactionHistory> TransactionHistories { get; } = new Repository<TransactionHistory>(dbContext);
    public IRepository<PracticeQuestion> PracticeQuestions { get; } = new Repository<PracticeQuestion>(dbContext);
    public IRepository<PracticeAttempt> PracticeAttempts { get; } = new Repository<PracticeAttempt>(dbContext);
    public IRepository<PracticeSavedQuestion> PracticeSavedQuestions { get; } = new Repository<PracticeSavedQuestion>(dbContext);
    public IRepository<PracticeQuotaPurchase> PracticeQuotaPurchases { get; } = new Repository<PracticeQuotaPurchase>(dbContext);
    public IRepository<PracticeTopic> PracticeTopics { get; } = new Repository<PracticeTopic>(dbContext);

    public async Task<bool> SaveChangesAsync()
    {
        return await dbContext.SaveChangesAsync() >= 0;
    }
}