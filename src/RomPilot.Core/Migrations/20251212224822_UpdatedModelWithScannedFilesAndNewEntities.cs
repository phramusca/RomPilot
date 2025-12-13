using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomPilot.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModelWithScannedFilesAndNewEntities : Migration
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
                name: "ExclusionFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FilterType = table.Column<string>(type: "TEXT", nullable: false),
                    FilterValue = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExclusionFilters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExportConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: false),
                    ExportType = table.Column<string>(type: "TEXT", nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", nullable: true),
                    SftpHost = table.Column<string>(type: "TEXT", nullable: true),
                    SftpPort = table.Column<int>(type: "INTEGER", nullable: false),
                    SftpUsername = table.Column<string>(type: "TEXT", nullable: true),
                    SftpAuthMethod = table.Column<string>(type: "TEXT", nullable: true),
                    SftpPasswordEncrypted = table.Column<string>(type: "TEXT", nullable: true),
                    SftpKeyPath = table.Column<string>(type: "TEXT", nullable: true),
                    RemoteDirectory = table.Column<string>(type: "TEXT", nullable: true),
                    FormatRequirement = table.Column<string>(type: "TEXT", nullable: true),
                    ConflictResolution = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastConnectionTest = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConnectionTestStatus = table.Column<string>(type: "TEXT", nullable: true),
                    ConnectionTestError = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceDatabases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Provider = table.Column<string>(type: "TEXT", nullable: false),
                    Console = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    ReleaseDate = table.Column<string>(type: "TEXT", nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: true),
                    DownloadStatus = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCheckedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceDatabases", x => x.Id);
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
                name: "GameEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReferenceDatabaseId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameName = table.Column<string>(type: "TEXT", nullable: false),
                    ConsoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    HashType = table.Column<string>(type: "TEXT", nullable: false),
                    HashValue = table.Column<string>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", nullable: true),
                    VideoFormat = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    QualityFlags = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    DatabaseSourceId = table.Column<int>(type: "INTEGER", nullable: true)
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GameEntries_ReferenceDatabases_ReferenceDatabaseId",
                        column: x => x.ReferenceDatabaseId,
                        principalTable: "ReferenceDatabases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScannedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    LastModifiedTimestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    ArchivePath = table.Column<string>(type: "TEXT", nullable: true),
                    ArchiveDepth = table.Column<int>(type: "INTEGER", nullable: false),
                    IdentificationStatus = table.Column<string>(type: "TEXT", nullable: false),
                    ExclusionReason = table.Column<string>(type: "TEXT", nullable: true),
                    FailureReason = table.Column<string>(type: "TEXT", nullable: true),
                    ConsoleId = table.Column<int>(type: "INTEGER", nullable: true),
                    GameEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastScannedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ScanType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScannedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScannedFiles_Consoles_ConsoleId",
                        column: x => x.ConsoleId,
                        principalTable: "Consoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScannedFiles_GameEntries_GameEntryId",
                        column: x => x.GameEntryId,
                        principalTable: "GameEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Checksums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScannedFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    HashType = table.Column<string>(type: "TEXT", nullable: false),
                    HashValue = table.Column<string>(type: "TEXT", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checksums", x => x.Id);
                    table.UniqueConstraint("AK_Checksums_ScannedFileId_HashType", x => new { x.ScannedFileId, x.HashType });
                    table.ForeignKey(
                        name: "FK_Checksums_ScannedFiles_ScannedFileId",
                        column: x => x.ScannedFileId,
                        principalTable: "ScannedFiles",
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
                    SelectedScannedFileId = table.Column<int>(type: "INTEGER", nullable: true),
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
                        name: "FK_Games_ScannedFiles_SelectedScannedFileId",
                        column: x => x.SelectedScannedFileId,
                        principalTable: "ScannedFiles",
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
                    ScannedFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSelected = table.Column<bool>(type: "INTEGER", nullable: false),
                    SelectionScore = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRomVersions", x => x.Id);
                    table.UniqueConstraint("AK_GameRomVersions_GameId_ScannedFileId", x => new { x.GameId, x.ScannedFileId });
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
                        name: "FK_GameRomVersions_ScannedFiles_ScannedFileId",
                        column: x => x.ScannedFileId,
                        principalTable: "ScannedFiles",
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
                name: "IX_Checksums_ScannedFileId",
                table: "Checksums",
                column: "ScannedFileId");

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
                name: "IX_ExclusionFilters_FilterType_FilterValue",
                table: "ExclusionFilters",
                columns: new[] { "FilterType", "FilterValue" });

            migrationBuilder.CreateIndex(
                name: "IX_ExclusionFilters_IsActive",
                table: "ExclusionFilters",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExportConfigurations_IsActive",
                table: "ExportConfigurations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExportConfigurations_Name",
                table: "ExportConfigurations",
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
                name: "IX_GameEntries_ReferenceDatabaseId",
                table: "GameEntries",
                column: "ReferenceDatabaseId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRomVersions_GameEntryId",
                table: "GameRomVersions",
                column: "GameEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRomVersions_GameId",
                table: "GameRomVersions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRomVersions_ScannedFileId",
                table: "GameRomVersions",
                column: "ScannedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_ConsoleId",
                table: "Games",
                column: "ConsoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SelectedDatabaseSourceId",
                table: "Games",
                column: "SelectedDatabaseSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SelectedScannedFileId",
                table: "Games",
                column: "SelectedScannedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_GameId",
                table: "Metadata",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceDatabases_DownloadStatus",
                table: "ReferenceDatabases",
                column: "DownloadStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceDatabases_Provider_Console_Version",
                table: "ReferenceDatabases",
                columns: new[] { "Provider", "Console", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScannedFiles_ConsoleId",
                table: "ScannedFiles",
                column: "ConsoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScannedFiles_FilePath_LastModifiedTimestamp",
                table: "ScannedFiles",
                columns: new[] { "FilePath", "LastModifiedTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ScannedFiles_GameEntryId",
                table: "ScannedFiles",
                column: "GameEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScannedFiles_IdentificationStatus",
                table: "ScannedFiles",
                column: "IdentificationStatus");

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
                name: "ExclusionFilters");

            migrationBuilder.DropTable(
                name: "ExportConfigurations");

            migrationBuilder.DropTable(
                name: "GameRomVersions");

            migrationBuilder.DropTable(
                name: "Metadata");

            migrationBuilder.DropTable(
                name: "UserData");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "ScannedFiles");

            migrationBuilder.DropTable(
                name: "GameEntries");

            migrationBuilder.DropTable(
                name: "Consoles");

            migrationBuilder.DropTable(
                name: "DatabaseSources");

            migrationBuilder.DropTable(
                name: "ReferenceDatabases");
        }
    }
}
