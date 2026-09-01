using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ZadElealm.Repository.Data.Datbases;

#nullable disable

namespace ZadElealm.Repository.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260831120000_AddLearningProgressUniqueness")]
public sealed class AddLearningProgressUniqueness : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ;WITH RankedVideoProgress AS (
                SELECT Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY UserId, VideoId
                        ORDER BY IsCompleted DESC, WatchedDuration DESC, CreatedAt DESC, Id DESC) AS RowNumber
                FROM VideoProgresses
                WHERE IsDeleted = 0
            )
            UPDATE progress
            SET IsDeleted = 1
            FROM VideoProgresses AS progress
            INNER JOIN RankedVideoProgress AS ranked ON ranked.Id = progress.Id
            WHERE ranked.RowNumber > 1;

            ;WITH RankedQuizProgress AS (
                SELECT Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY AppUserId, QuizId
                        ORDER BY IsCompleted DESC, Score DESC, CreatedAt DESC, Id DESC) AS RowNumber
                FROM Progresses
                WHERE IsDeleted = 0
            )
            UPDATE progress
            SET IsDeleted = 1
            FROM Progresses AS progress
            INNER JOIN RankedQuizProgress AS ranked ON ranked.Id = progress.Id
            WHERE ranked.RowNumber > 1;

            ;WITH RankedCertificates AS (
                SELECT Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY UserId, QuizId
                        ORDER BY CreatedAt ASC, Id ASC) AS RowNumber
                FROM Certificates
                WHERE IsDeleted = 0
            )
            UPDATE certificate
            SET IsDeleted = 1
            FROM Certificates AS certificate
            INNER JOIN RankedCertificates AS ranked ON ranked.Id = certificate.Id
            WHERE ranked.RowNumber > 1;
            """);

        migrationBuilder.CreateIndex(
            name: "UX_VideoProgresses_UserId_VideoId",
            table: "VideoProgresses",
            columns: new[] { "UserId", "VideoId" },
            unique: true,
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "UX_Progresses_AppUserId_QuizId",
            table: "Progresses",
            columns: new[] { "AppUserId", "QuizId" },
            unique: true,
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "UX_Certificates_UserId_QuizId",
            table: "Certificates",
            columns: new[] { "UserId", "QuizId" },
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UX_VideoProgresses_UserId_VideoId",
            table: "VideoProgresses");

        migrationBuilder.DropIndex(
            name: "UX_Progresses_AppUserId_QuizId",
            table: "Progresses");

        migrationBuilder.DropIndex(
            name: "UX_Certificates_UserId_QuizId",
            table: "Certificates");
    }
}
