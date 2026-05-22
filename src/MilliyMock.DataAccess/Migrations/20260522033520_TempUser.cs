using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TempUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTestAttempts_Users_UserId",
                table: "UserTestAttempts");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserTestAttempts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "TempUserId",
                table: "UserTestAttempts",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TempUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    FatherName = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempUsers", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 22, 8, 35, 20, 95, DateTimeKind.Utc).AddTicks(130), "$2a$11$tE4adflNbMp/1Kvw2l7y..bAAFeh8xzLuHrknUDszNEJ/BWrAj.1K" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTestAttempts_TempUserId",
                table: "UserTestAttempts",
                column: "TempUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTestAttempts_TempUsers_TempUserId",
                table: "UserTestAttempts",
                column: "TempUserId",
                principalTable: "TempUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTestAttempts_Users_UserId",
                table: "UserTestAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTestAttempts_TempUsers_TempUserId",
                table: "UserTestAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTestAttempts_Users_UserId",
                table: "UserTestAttempts");

            migrationBuilder.DropTable(
                name: "TempUsers");

            migrationBuilder.DropIndex(
                name: "IX_UserTestAttempts_TempUserId",
                table: "UserTestAttempts");

            migrationBuilder.DropColumn(
                name: "TempUserId",
                table: "UserTestAttempts");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserTestAttempts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 17, 17, 9, 14, 63, DateTimeKind.Utc).AddTicks(7460), "$2a$11$GXjb7tHScFrIrq7shFWwp.0WQQU6H9JqUA.PK.Y33NBbhAoy1YA1C" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserTestAttempts_Users_UserId",
                table: "UserTestAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
