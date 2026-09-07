using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZadElealm.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTimedAssessmentAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssessmentQuestions_AssessmentFormId",
                table: "AssessmentQuestions");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "AssessmentQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "AssessmentQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttemptExpiresAtUtc",
                table: "AssessmentProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttemptStartedAtUtc",
                table: "AssessmentProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttemptSubmittedAtUtc",
                table: "AssessmentProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                """
                WITH RankedQuestions AS
                (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY AssessmentFormId ORDER BY Id) AS QuestionOrder
                    FROM AssessmentQuestions
                    WHERE IsDeleted = 0
                )
                UPDATE questions
                SET DisplayOrder = ranked.QuestionOrder,
                    Difficulty = CASE
                        WHEN ranked.QuestionOrder <= 10 THEN 1
                        WHEN ranked.QuestionOrder <= 20 THEN 2
                        ELSE 3
                    END
                FROM AssessmentQuestions AS questions
                INNER JOIN RankedQuestions AS ranked ON ranked.Id = questions.Id;
                """);

            migrationBuilder.CreateIndex(
                name: "UX_AssessmentQuestions_FormId_DisplayOrder",
                table: "AssessmentQuestions",
                columns: new[] { "AssessmentFormId", "DisplayOrder" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_AssessmentQuestions_FormId_DisplayOrder",
                table: "AssessmentQuestions");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "AssessmentQuestions");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "AssessmentQuestions");

            migrationBuilder.DropColumn(
                name: "AttemptExpiresAtUtc",
                table: "AssessmentProgresses");

            migrationBuilder.DropColumn(
                name: "AttemptStartedAtUtc",
                table: "AssessmentProgresses");

            migrationBuilder.DropColumn(
                name: "AttemptSubmittedAtUtc",
                table: "AssessmentProgresses");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_AssessmentFormId",
                table: "AssessmentQuestions",
                column: "AssessmentFormId");
        }
    }
}
