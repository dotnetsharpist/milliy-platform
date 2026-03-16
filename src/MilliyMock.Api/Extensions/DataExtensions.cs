using MilliyMock.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MilliyMock.Extensions
{
    public static class DataExtensions
    {
        /// <summary>
        /// Automatically updated database based on latest migration
        /// </summary>
        /// <param name="app"></param>
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MilliyMockDbContext>();

            db.Database.Migrate();
        }
    }
}
