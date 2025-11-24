using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomPilot.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessingStatusAndFailureReasonToRomFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingStatus",
                table: "RomFiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "RomFiles",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "RomFiles");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "RomFiles");
        }
    }
}

