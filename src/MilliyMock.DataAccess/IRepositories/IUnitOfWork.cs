using MilliyMock.Domain.Entities;

namespace MilliyMock.DataAccess.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<TempUser> TempUsers { get; }
    IRepository<BotUser> BotUsers { get; }
    IRepository<Test> Tests { get; }
    IRepository<QuestionGroup> QuestionGroups { get; }
    IRepository<Question> Questions { get; }
    IRepository<Translation> Translations { get; }
    IRepository<QuestionExplanation> QuestionExplanations { get; }
    IRepository<Option> Options { get; }
    IRepository<UserTestAttempt> UserTestAttempts { get; }
    IRepository<UserAnswer> UserAnswer { get; }
    IRepository<UserBalance> UserBalances { get; }
    IRepository<TransactionHistory> TransactionHistories { get; }
    IRepository<PracticeQuestion> PracticeQuestions { get; }
    IRepository<PracticeAttempt> PracticeAttempts { get; }
    IRepository<PracticeSavedQuestion> PracticeSavedQuestions { get; }
    IRepository<PracticeQuotaPurchase> PracticeQuotaPurchases { get; }
    IRepository<PracticeTopic> PracticeTopics { get; }
    Task<bool> SaveChangesAsync();
}