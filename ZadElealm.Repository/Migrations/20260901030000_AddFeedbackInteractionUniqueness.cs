using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ZadElealm.Repository.Data.Datbases;

#nullable disable

namespace ZadElealm.Repository.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260901030000_AddFeedbackInteractionUniqueness")]
public sealed class AddFeedbackInteractionUniqueness : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ;WITH RankedReviewLikes AS (
                SELECT Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY AppUserId, ReviewId
                        ORDER BY CreatedAt DESC, Id DESC) AS RowNumber
                FROM ReviewLikes
            )
            DELETE reviewLike
            FROM ReviewLikes AS reviewLike
            INNER JOIN RankedReviewLikes AS ranked ON ranked.Id = reviewLike.Id
            WHERE ranked.RowNumber > 1;

            ;WITH RankedReplyLikes AS (
                SELECT Id,
                    ROW_NUMBER() OVER (
                        PARTITION BY AppUserId, ReplyId
                        ORDER BY CreatedAt DESC, Id DESC) AS RowNumber
                FROM ReplyLike
            )
            DELETE replyLike
            FROM ReplyLike AS replyLike
            INNER JOIN RankedReplyLikes AS ranked ON ranked.Id = replyLike.Id
            WHERE ranked.RowNumber > 1;
            """);

        migrationBuilder.CreateIndex(
            name: "UX_ReviewLikes_AppUserId_ReviewId",
            table: "ReviewLikes",
            columns: new[] { "AppUserId", "ReviewId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_ReplyLike_AppUserId_ReplyId",
            table: "ReplyLike",
            columns: new[] { "AppUserId", "ReplyId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "UX_ReviewLikes_AppUserId_ReviewId",
            table: "ReviewLikes");

        migrationBuilder.DropIndex(
            name: "UX_ReplyLike_AppUserId_ReplyId",
            table: "ReplyLike");
    }
}
