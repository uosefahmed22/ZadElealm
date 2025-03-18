using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZadElealm.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Update_VideoProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoProgresses_AspNetUsers_UserId",
                table: "VideoProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoProgresses_Videos_VideoId",
                table: "VideoProgresses");

            migrationBuilder.DropIndex(
                name: "IX_VideoProgresses_UserId",
                table: "VideoProgresses");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "VideoProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VideoProgresses_CourseId",
                table: "VideoProgresses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoProgresses_UserId_CourseId",
                table: "VideoProgresses",
                columns: new[] { "UserId", "CourseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_VideoProgresses_AspNetUsers_UserId",
                table: "VideoProgresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoProgresses_Courses_CourseId",
                table: "VideoProgresses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoProgresses_Videos_VideoId",
                table: "VideoProgresses",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoProgresses_AspNetUsers_UserId",
                table: "VideoProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoProgresses_Courses_CourseId",
                table: "VideoProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_VideoProgresses_Videos_VideoId",
                table: "VideoProgresses");

            migrationBuilder.DropIndex(
                name: "IX_VideoProgresses_CourseId",
                table: "VideoProgresses");

            migrationBuilder.DropIndex(
                name: "IX_VideoProgresses_UserId_CourseId",
                table: "VideoProgresses");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "VideoProgresses");

            migrationBuilder.CreateIndex(
                name: "IX_VideoProgresses_UserId",
                table: "VideoProgresses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoProgresses_AspNetUsers_UserId",
                table: "VideoProgresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VideoProgresses_Videos_VideoId",
                table: "VideoProgresses",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
