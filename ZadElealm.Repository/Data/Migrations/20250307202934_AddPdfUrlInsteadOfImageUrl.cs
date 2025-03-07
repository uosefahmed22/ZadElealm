using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZadElealm.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfUrlInsteadOfImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Certificates",
                newName: "PdfUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PdfUrl",
                table: "Certificates",
                newName: "ImageUrl");
        }
    }
}
