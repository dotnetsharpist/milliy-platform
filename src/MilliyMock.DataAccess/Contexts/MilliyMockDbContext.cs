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
            .HasIndex(u => u.Username)
            .IsUnique();
        
        var hasUsers = modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1, FullName = "Abdurrohman", Username = "ysharpist",
                CurrentGrade = "11", PasswordHash = PasswordHelper.Hash("nigga"),
                Role = UserRole.SuperAdmin, CreatedBy = 1
            }
        );
    }
    
    public DbSet<User> Users { get; init; }
}