using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class BotUserTgUserIdIsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 6, 16, 14, 53, 14, 168, DateTimeKind.Utc).AddTicks(5600), "$2a$11$lgpsTL1/cUMroVrE0caNp.quuPbM3g/bRMaK/tqZVQVmtLW.3fA0G" });

            migrationBuilder.CreateIndex(
                name: "IX_BotUsers_TgUserId",
                table: "BotUsers",
                column: "TgUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BotUsers_TgUserId",
                table: "BotUsers");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 6, 16, 14, 52, 44, 967, DateTimeKind.Utc).AddTicks(8890), "$2a$11$B4W8hzFvq1mCweX3gz4rWOWLzhb9xWt82g2wZinAhFiBRWi6mkkae" });
        }
    }
}
