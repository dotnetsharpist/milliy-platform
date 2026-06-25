using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilliyMock.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAnswerUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserTestAttemptId",
                table: "UserAnswers");

            // Collapse duplicate answers (left behind by the old insert-on-update bug) before
            // the unique index is created: keep the most recent row (highest Id) per
            // (attempt, question) among non-deleted rows, matching the partial index filter.
            migrationBuilder.Sql(
                """
                DELETE FROM "UserAnswers" a
                USING "UserAnswers" b
                WHERE a."UserTestAttemptId" = b."UserTestAttemptId"
                  AND a."QuestionId" = b."QuestionId"
                  AND a."IsDeleted" = false
                  AND b."IsDeleted" = false
                  AND a."Id" < b."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserTestAttemptId_QuestionId",
                table: "UserAnswers",
                columns: new[] { "UserTestAttemptId", "QuestionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserTestAttemptId_QuestionId",
                table: "UserAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserTestAttemptId",
                table: "UserAnswers",
                column: "UserTestAttemptId");
        }
    }
}
