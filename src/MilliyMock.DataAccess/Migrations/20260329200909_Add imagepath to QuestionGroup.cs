using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddimagepathtoQuestionGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "QuestionGroups",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 30, 1, 9, 9, 665, DateTimeKind.Utc).AddTicks(4680), "$2a$11$y.g2UkL6GBLjlBUgkMUtu.iQTA2ejPJKGCa50vi9CuP0Y5U6jYhKS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "QuestionGroups");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 24, 23, 5, 36, 459, DateTimeKind.Utc).AddTicks(7350), "$2a$11$xRA9ByZiowTpAoFBSRLLuOoWJpNI/XptZeLKcB2hqJYxiU822iCnO" });
        }
    }
}
