using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomPilot.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", nullable: false),
                    RecalboxFolderName = table.Column<string>(type: "TEXT", nullable: false),
                    RommPlatformId = table.Column<string>(type: "TEXT", nullable: true),
                    SupportedFormats = table.Column<string>(type: "TEXT", nullable: true),
                    ExportFormat = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DatfilePath = table.Column<string>(type: "TEXT", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RomFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    ArchivePath = table.Column<string>(type: "TEXT", nullable: true),
                    ArchiveDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastScannedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RomFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RomFiles_Consoles_ConsoleId",
                        column: x => x.ConsoleId,
                        principalTable: "Consoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DatabaseSourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameName = table.Column<string>(type: "TEXT", nullable: false),
                    ConsoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    HashType = table.Column<string>(type: "TEXT", nullable: false),
                    HashValue = table.Column<string>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    QualityFlags = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameEntries_Consoles_ConsoleId",
                        column: x => x.ConsoleId,
                        principalTable: "Consoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameEntries_DatabaseSources_DatabaseSourceId",
                        column: x => x.DatabaseSourceId,
                        principalTable: "DatabaseSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Checksums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RomFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    HashType = table.Column<string>(type: "TEXT", nullable: false),
                    HashValue = table.Column<string>(type: "TEXT", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checksums", x => x.Id);
                    table.UniqueConstraint("AK_Checksums_RomFileId_HashType", x => new { x.RomFileId, x.HashType });
                    table.ForeignKey(
                        name: "FK_Checksums_RomFiles_RomFileId",
                        column: x => x.RomFileId,
                        principalTable: "RomFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ConsoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectedDatabaseSourceId = table.Column<int>(type: "INTEGER", nullable: true),
                    SelectedRomFileId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Consoles_ConsoleId",
                        column: x => x.ConsoleId,
                        principalTable: "Consoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_DatabaseSources_SelectedDatabaseSourceId",
                        column: x => x.SelectedDatabaseSourceId,
                        principalTable: "DatabaseSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Games_RomFiles_SelectedRomFileId",
                        column: x => x.SelectedRomFileId,
                        principalTable: "RomFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GameRomVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    RomFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSelected = table.Column<bool>(type: "INTEGER", nullable: false),
                    SelectionScore = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRomVersions", x => x.Id);
                    table.UniqueConstraint("AK_GameRomVersions_GameId_RomFileId", x => new { x.GameId, x.RomFileId });
                    table.ForeignKey(
                        name: "FK_GameRomVersions_GameEntries_GameEntryId",
                        column: x => x.GameEntryId,
                        principalTable: "GameEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GameRomVersions_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRomVersions_RomFiles_RomFileId",
                        column: x => x.RomFileId,
                        principalTable: "RomFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Metadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Rating = table.Column<double>(type: "REAL", nullable: true),
                    ReleaseDate = table.Column<string>(type: "TEXT", nullable: true),
                    Developer = table.Column<string>(type: "TEXT", nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", nullable: true),
                    Genre = table.Column<string>(type: "TEXT", nullable: true),
                    CoverImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    ThumbnailPath = table.Column<string>(type: "TEXT", nullable: true),
                    ScreenshotPaths = table.Column<string>(type: "TEXT", nullable: true),
                    VideoPath = table.Column<string>(type: "TEXT", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metadata_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserRating = table.Column<double>(type: "REAL", nullable: true),
                    PlayCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimePlayed = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserData_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checksums_HashType_HashValue",
                table: "Checksums",
                columns: new[] { "HashType", "HashValue" });

            migrationBuilder.CreateIndex(
                name: "IX_Checksums_RomFileId",
                table: "Checksums",
                column: "RomFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Consoles_Name",
                table: "Consoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consoles_ShortName",
                table: "Consoles",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseSources_Name",
                table: "DatabaseSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameEntries_ConsoleId",
                table: "GameEntries",
                column: "ConsoleId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEntries_DatabaseSourceId",
                table: "GameEntries",
                column: "DatabaseSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEntries_HashType_HashValue",
                table: "GameEntries",
                columns: new[] { "HashType", "HashValue" });

            migrationBuilder.CreateIndex(
                name: "IX_GameRomVersions_GameEntryId",
                table: "GameRomVersions",
                column: "GameEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRomVersions_GameId",
                table: "GameRomVersions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRomVersions_RomFileId",
                table: "GameRomVersions",
                column: "RomFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_ConsoleId",
                table: "Games",
                column: "ConsoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SelectedDatabaseSourceId",
                table: "Games",
                column: "SelectedDatabaseSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SelectedRomFileId",
                table: "Games",
                column: "SelectedRomFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_GameId",
                table: "Metadata",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_RomFiles_ConsoleId",
                table: "RomFiles",
                column: "ConsoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_GameId",
                table: "UserData",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserData_IsFavorite",
                table: "UserData",
                column: "IsFavorite");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_Key",
                table: "UserPreferences",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checksums");

            migrationBuilder.DropTable(
                name: "GameRomVersions");

            migrationBuilder.DropTable(
                name: "Metadata");

            migrationBuilder.DropTable(
                name: "UserData");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "GameEntries");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "DatabaseSources");

            migrationBuilder.DropTable(
                name: "RomFiles");

            migrationBuilder.DropTable(
                name: "Consoles");
        }
    }
}
