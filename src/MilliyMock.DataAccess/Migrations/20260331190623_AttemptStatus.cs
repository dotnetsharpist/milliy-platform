using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AttemptStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptStatus",
                table: "UserTestAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeSpent",
                table: "UserTestAttempts",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 1, 0, 6, 23, 488, DateTimeKind.Utc).AddTicks(880), "$2a$11$PbdTwfj7kD0/3birOomY1.bpvDgs0I1f6yBK5.nfR5iokR67PM5by" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptStatus",
                table: "UserTestAttempts");

            migrationBuilder.DropColumn(
                name: "TimeSpent",
                table: "UserTestAttempts");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 30, 1, 9, 9, 665, DateTimeKind.Utc).AddTicks(4680), "$2a$11$y.g2UkL6GBLjlBUgkMUtu.iQTA2ejPJKGCa50vi9CuP0Y5U6jYhKS" });
        }
    }
}
