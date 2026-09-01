using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ZadElealm.Repository.Data.Datbases;

#nullable disable

namespace ZadElealm.Repository.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260901020000_AddCourseFeedbackUniqueness")]
public sealed class AddCourseFeedbackUniqueness : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ;WITH RankedReviews AS (
                SELECT Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY AppUserId, CourseId
                        ORDER BY CreatedAt DESC, Id DESC) AS RowNumber
                FROM Reviews
                WHERE IsDeleted = 0
            )
            UPDATE review
            SET IsDeleted = 1
            FROM Reviews AS review
            INNER JOIN RankedReviews AS ranked ON ranked.Id = review.Id
            WHERE ranked.RowNumber > 1;

            ;WITH RankedRatings AS (
                SELECT Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY AppUserId, courseId
                        ORDER BY CreatedAt DESC, Id DESC) AS RowNumber
                FROM Ratings
                WHERE IsDeleted = 0
            )
            UPDATE rating
            SET IsDeleted = 1
            FROM Ratings AS rating
            INNER JOIN RankedRatings AS ranked ON ranked.Id = rating.Id
            WHERE ranked.RowNumber > 1;

            UPDATE courseItem
            SET rating = COALESCE(averageRating.Value, 0)
            FROM Courses AS courseItem
            OUTER APPLY (
                SELECT AVG(ratingItem.Value) AS Value
                FROM Ratings AS ratingItem
                WHERE ratingItem.courseId = courseItem.Id AND ratingItem.IsDeleted = 0
            ) AS averageRating;
            """);

        migrationBuilder.CreateIndex(
            name: "UX_Reviews_AppUserId_CourseId",
            table: "Reviews",
            columns: new[] { "AppUserId", "CourseId" },
            unique: true,
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "UX_Ratings_AppUserId_courseId",
            table: "Ratings",
            columns: new[] { "AppUserId", "courseId" },
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UX_Reviews_AppUserId_CourseId",
            table: "Reviews");

        migrationBuilder.DropIndex(
            name: "UX_Ratings_AppUserId_courseId",
            table: "Ratings");
    }
}
