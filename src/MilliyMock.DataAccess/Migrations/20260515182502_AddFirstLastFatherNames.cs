using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstLastFatherNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "FatherName", "FirstName", "LastName", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 15, 23, 25, 1, 983, DateTimeKind.Utc).AddTicks(4230), null, "Abdurrohman", null, "$2a$11$HA7bDo6MCavPCB832YIMMe4xljxkhF/Dn9J2lZYq7b1FA63fEqDT." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 5, 23, 1, 25, 780, DateTimeKind.Utc).AddTicks(7810), "$2a$11$nkDWVTP.zdwrpJiHgOOyguxdprkm6hQLtSyhP6tsTxnNGmEbU3eDW" });
        }
    }
}
