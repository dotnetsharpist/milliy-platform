using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TestDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tests",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "Email", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 2, 0, 54, 50, 724, DateTimeKind.Utc).AddTicks(9950), "ysharpist@gmail.com", "$2a$11$4VafmAxZ49lYSfFJRgi6zuz3c5p8/KX8rR8jNKSwfT.ZtnKKNmeaS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tests");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "CreatedAt", "Email", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 1, 0, 6, 23, 488, DateTimeKind.Utc).AddTicks(880), "ysharpist", "$2a$11$PbdTwfj7kD0/3birOomY1.bpvDgs0I1f6yBK5.nfR5iokR67PM5by" });
        }
    }
}
