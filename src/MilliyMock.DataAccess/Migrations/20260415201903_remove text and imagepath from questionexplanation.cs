using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removetextandimagepathfromquestionexplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "QuestionExplanations");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "QuestionExplanations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 16, 1, 19, 3, 285, DateTimeKind.Utc).AddTicks(4730), "$2a$11$Or7lCIKsCGvcY2d95CLM7uHZmvfNFvDhQeOaKv5wpkuC.UIo07Gd6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "QuestionExplanations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "QuestionExplanations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 8, 19, 11, 14, 573, DateTimeKind.Utc).AddTicks(3550), "$2a$11$UQWwfuKNfIgs0eIQgl8k.ewmEJ9yeZEc0lLyV8oXOhpQZgYAJu1hy" });
        }
    }
}
