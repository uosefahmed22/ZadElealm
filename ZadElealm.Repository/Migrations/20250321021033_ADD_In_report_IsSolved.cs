using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZadElealm.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ADD_In_report_IsSolved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSolved",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSolved",
                table: "Reports");
        }
    }
}
