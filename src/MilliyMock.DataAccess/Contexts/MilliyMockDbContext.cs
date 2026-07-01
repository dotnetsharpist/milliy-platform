using System.Linq.Expressions;
using MilliyMock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MilliyMock.Domain.Commons;

namespace MilliyMock.DataAccess.Contexts;

public class MilliyMockDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Auditable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(BuildIsDeletedFilter(entityType.ClrType));
            }
        }

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.PhoneNumber)
            .HasMaxLength(15);

        modelBuilder.Entity<BotUser>()
            .HasIndex(bu => bu.TgUserId)
            .IsUnique();

        modelBuilder.Entity<UserBalance>()
            .HasIndex(b => b.UserId)
            .IsUnique();
        

        // One answer per (attempt, question). Partial filter mirrors the soft-delete
        // query filter so a re-answer after a soft delete doesn't collide.
        modelBuilder.Entity<UserAnswer>()
            .HasIndex(ua => new { ua.UserTestAttemptId, ua.QuestionId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        /*
        var hasUsers = modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1, FirstName = "Abdurrohman", Email = "ysharpist@gmail.com",
                PasswordHash = PasswordHelper.Hash(configuration.GetSection("SuperAdminPassword").Value!),
                Role = UserRole.SuperAdmin, CreatedBy = 1
            }
        );
    */
    }
    
    
    private static LambdaExpression BuildIsDeletedFilter(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var body = Expression.Equal(
            Expression.Property(param, nameof(Auditable.IsDeleted)),
            Expression.Constant(false)
        );
        return Expression.Lambda(body, param);
    }

    
    public DbSet<User> Users { get; init; }
    public DbSet<TempUser> TempUsers { get; init; }
    public DbSet<BotUser> BotUsers { get; init; }
    public DbSet<Test> Tests { get; init; }
    public DbSet<QuestionGroup> QuestionGroups { get; init; }
    public DbSet<Question> Questions { get; init; }
    public DbSet<Translation> Translations { get; init; }
    public DbSet<QuestionExplanation> QuestionExplanations { get; init; }
    public DbSet<Option> Options { get; init; }
    public DbSet<UserTestAttempt> UserTestAttempts { get; init; }
    public DbSet<UserAnswer> UserAnswers { get; init; }
    public DbSet<UserBalance> UserBalances { get; init; }
    public DbSet<TransactionHistory> TransactionHistories { get; init; }
}