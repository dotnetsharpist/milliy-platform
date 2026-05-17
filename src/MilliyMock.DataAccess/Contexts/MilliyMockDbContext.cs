using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;
using MilliyMock.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MilliyMock.DataAccess.Contexts;

public class MilliyMockDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        var hasUsers = modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1, FirstName = "Abdurrohman", Email = "ysharpist@gmail.com",
                PasswordHash = PasswordHelper.Hash("nigga"),
                Role = UserRole.SuperAdmin, CreatedBy = 1
            }
        );
    }
    
    public DbSet<User> Users { get; init; }
    public DbSet<BotUser> BotUsers { get; init; }
    public DbSet<Test> Tests { get; init; }
    public DbSet<QuestionGroup> QuestionGroups { get; init; }
    public DbSet<Question> Questions { get; init; }
    public DbSet<Translation> Translations { get; init; }
    public DbSet<QuestionExplanation> QuestionExplanations { get; init; }
    public DbSet<Option> Options { get; init; }
    public DbSet<UserTestAttempt> UserTestAttempts { get; init; }
    public DbSet<UserAnswer> UserAnswers { get; init; }
}