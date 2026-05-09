using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class VideoLinkForQuestionExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoLink",
                table: "QuestionExplanations",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 5, 23, 1, 25, 780, DateTimeKind.Utc).AddTicks(7810), "$2a$11$nkDWVTP.zdwrpJiHgOOyguxdprkm6hQLtSyhP6tsTxnNGmEbU3eDW" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoLink",
                table: "QuestionExplanations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 4, 23, 34, 22, 670, DateTimeKind.Utc).AddTicks(2170), "$2a$11$yA2vmeb4XjSO6lC3nv4hD.HGrFns5NJwjjoxtpHKzTE1dytnmoJ.q" });
        }
    }
}
