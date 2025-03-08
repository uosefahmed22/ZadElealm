using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZadElealm.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class makecorrectchoicinteger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CorrectChoice",
                table: "Question",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CorrectChoice",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
