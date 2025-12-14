using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomPilot.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathInArchiveToScannedFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePathInArchive",
                table: "ScannedFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePathInArchive",
                table: "ScannedFiles");
        }
    }
}
