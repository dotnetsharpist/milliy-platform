using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFullname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionExplanations_QuestionGroupId",
                table: "QuestionExplanations");

            migrationBuilder.DropIndex(
                name: "IX_QuestionExplanations_QuestionId",
                table: "QuestionExplanations");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 17, 17, 9, 14, 63, DateTimeKind.Utc).AddTicks(7460), "$2a$11$GXjb7tHScFrIrq7shFWwp.0WQQU6H9JqUA.PK.Y33NBbhAoy1YA1C" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExplanations_QuestionGroupId",
                table: "QuestionExplanations",
                column: "QuestionGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExplanations_QuestionId",
                table: "QuestionExplanations",
                column: "QuestionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionExplanations_QuestionGroupId",
                table: "QuestionExplanations");

            migrationBuilder.DropIndex(
                name: "IX_QuestionExplanations_QuestionId",
                table: "QuestionExplanations");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "FullName", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 15, 23, 25, 1, 983, DateTimeKind.Utc).AddTicks(4230), "Abdurrohman", "$2a$11$HA7bDo6MCavPCB832YIMMe4xljxkhF/Dn9J2lZYq7b1FA63fEqDT." });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExplanations_QuestionGroupId",
                table: "QuestionExplanations",
                column: "QuestionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionExplanations_QuestionId",
                table: "QuestionExplanations",
                column: "QuestionId");
        }
    }
}
