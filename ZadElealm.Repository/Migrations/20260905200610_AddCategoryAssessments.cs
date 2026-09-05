using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZadElealm.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Quizzes_QuizId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "UX_Certificates_UserId_QuizId",
                table: "Certificates");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "Certificates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AssessmentId",
                table: "Certificates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PassingScore = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentForms_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    AssessmentId = table.Column<int>(type: "int", nullable: false),
                    AssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentProgresses_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentProgresses_AssessmentForms_AssessmentFormId",
                        column: x => x.AssessmentFormId,
                        principalTable: "AssessmentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentProgresses_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AssessmentFormId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestions_AssessmentForms_AssessmentFormId",
                        column: x => x.AssessmentFormId,
                        principalTable: "AssessmentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AssessmentQuestionId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentChoices_AssessmentQuestions_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_AssessmentId",
                table: "Certificates",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "UX_Certificates_UserId_AssessmentId",
                table: "Certificates",
                columns: new[] { "UserId", "AssessmentId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [AssessmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Certificates_UserId_QuizId",
                table: "Certificates",
                columns: new[] { "UserId", "QuizId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [QuizId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentChoices_AssessmentQuestionId",
                table: "AssessmentChoices",
                column: "AssessmentQuestionId");

            migrationBuilder.CreateIndex(
                name: "UX_AssessmentForms_AssessmentId_InternalCode",
                table: "AssessmentForms",
                columns: new[] { "AssessmentId", "InternalCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentProgresses_AssessmentFormId",
                table: "AssessmentProgresses",
                column: "AssessmentFormId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentProgresses_AssessmentId",
                table: "AssessmentProgresses",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "UX_AssessmentProgresses_AppUserId_AssessmentId",
                table: "AssessmentProgresses",
                columns: new[] { "AppUserId", "AssessmentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_AssessmentFormId",
                table: "AssessmentQuestions",
                column: "AssessmentFormId");

            migrationBuilder.CreateIndex(
                name: "UX_Assessments_CategoryId",
                table: "Assessments",
                column: "CategoryId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Assessments_AssessmentId",
                table: "Certificates",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Quizzes_QuizId",
                table: "Certificates",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Assessments_AssessmentId",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Quizzes_QuizId",
                table: "Certificates");

            migrationBuilder.DropTable(
                name: "AssessmentChoices");

            migrationBuilder.DropTable(
                name: "AssessmentProgresses");

            migrationBuilder.DropTable(
                name: "AssessmentQuestions");

            migrationBuilder.DropTable(
                name: "AssessmentForms");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_AssessmentId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "UX_Certificates_UserId_AssessmentId",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "UX_Certificates_UserId_QuizId",
                table: "Certificates");

            migrationBuilder.Sql("DELETE FROM [Certificates] WHERE [QuizId] IS NULL");

            migrationBuilder.DropColumn(
                name: "AssessmentId",
                table: "Certificates");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Certificates_UserId_QuizId",
                table: "Certificates",
                columns: new[] { "UserId", "QuizId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Quizzes_QuizId",
                table: "Certificates",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        }
    }
}
