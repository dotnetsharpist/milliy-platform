using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTestDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Tests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Tests");
        }
    }
}
