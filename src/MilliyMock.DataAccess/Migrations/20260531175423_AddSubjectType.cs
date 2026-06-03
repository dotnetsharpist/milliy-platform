using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Subject",
                table: "Tests",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 31, 22, 54, 22, 683, DateTimeKind.Utc).AddTicks(8320), "$2a$11$5LYBuwpGYFzKKPRvg3PRV.cZQL02weqsWqkoCaoCE0CqzYYEs.Yoa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Tests");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 22, 8, 35, 20, 95, DateTimeKind.Utc).AddTicks(130), "$2a$11$tE4adflNbMp/1Kvw2l7y..bAAFeh8xzLuHrknUDszNEJ/BWrAj.1K" });
        }
    }
}
