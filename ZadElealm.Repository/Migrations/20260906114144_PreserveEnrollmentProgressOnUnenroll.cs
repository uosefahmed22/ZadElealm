using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZadElealm.Repository.Migrations
{
    /// <inheritdoc />
    public partial class PreserveEnrollmentProgressOnUnenroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_AppUserId",
                table: "Enrollments");

            migrationBuilder.AddColumn<DateTime>(
                name: "UnenrolledAtUtc",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            // Preserve historical rows while making the active enrollment unique.
            // If older races created duplicate active rows, keep the newest one active
            // and soft-delete the others before the filtered unique index is created.
            migrationBuilder.Sql(
                """
                WITH DuplicateActiveEnrollments AS
                (
                    SELECT [Id],
                           ROW_NUMBER() OVER
                           (
                               PARTITION BY [AppUserId], [CourseId]
                               ORDER BY [CreatedAt] DESC, [Id] DESC
                           ) AS [RowNumber]
                    FROM [Enrollments]
                    WHERE [IsDeleted] = 0
                )
                UPDATE [enrollment]
                SET [enrollment].[IsDeleted] = 1,
                    [enrollment].[UnenrolledAtUtc] = COALESCE(
                        [enrollment].[UnenrolledAtUtc],
                        SYSUTCDATETIME())
                FROM [Enrollments] AS [enrollment]
                INNER JOIN [DuplicateActiveEnrollments] AS [duplicate]
                    ON [duplicate].[Id] = [enrollment].[Id]
                WHERE [duplicate].[RowNumber] > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "UX_Enrollments_AppUserId_CourseId",
                table: "Enrollments",
                columns: new[] { "AppUserId", "CourseId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Enrollments_AppUserId_CourseId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "UnenrolledAtUtc",
                table: "Enrollments");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_AppUserId",
                table: "Enrollments",
                column: "AppUserId");
        }
    }
}
