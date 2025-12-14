using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Archives;
using RomPilot.Core.Checksums;
using RomPilot.Core.Database;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;
using RomPilot.Core.Services;
using RomPilot.Tests.Helpers;
using Xunit;

namespace RomPilot.Tests.Integration;

/// <summary>
/// Tests d'intégration pour vérifier le support des archives 7Z et RAR,
/// ainsi que les archives imbriquées avec formats mixtes (ZIP, 7Z, RAR).
/// </summary>
public class US1_MixedArchiveFormatTests : IDisposable
{
    private readonly RomPilotDbContext _context;
    private readonly IScanService _scanService;
    private readonly IFilterService _filterService;
    private readonly string _testDirectory;
    private readonly ScanProgressReporter _progressReporter;

    public US1_MixedArchiveFormatTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<RomPilotDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        _context = new RomPilotDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        // Seed test data
        SeedTestData();

        // Ensure test data files are generated
        TestDataHelper.EnsureTestDataGeneratedAsync().Wait();

        // Setup services
        var archiveScanner = new ArchiveScanner();
        var checksumCalculator = new ChecksumCalculator();
        var consoleRepository = new ConsoleRepository(_context);
        var consoleDetectionService = new ConsoleDetectionService(consoleRepository);
        var gameIdentificationService = new GameIdentificationService(_context);
        var scannedFileRepository = new ScannedFileRepository(_context);
        var checksumRepository = new ChecksumRepository(_context);
        var exclusionFilterRepository = new ExclusionFilterRepository(_context);
        _filterService = new FilterService(exclusionFilterRepository);

        // Initialize default exclusion filters
        _filterService.InitializeDefaultFiltersAsync().Wait();

        _scanService = new ScanService(
            _context,
            archiveScanner,
            checksumCalculator,
            consoleDetectionService,
            gameIdentificationService,
            scannedFileRepository,
            checksumRepository,
            _filterService);

        _progressReporter = new ScanProgressReporter();

        // Use test data directory
        _testDirectory = TestDataHelper.GetTestDataRoot();
    }

    private void SeedTestData()
    {
        // Add test consoles
        _context.Consoles.Add(new RomPilot.Core.Models.Console
        {
            Id = 1,
            Name = "Nintendo Entertainment System",
            ShortName = "nes",
            RecalboxFolderName = "nes",
            SupportedFormats = "[\".nes\"]",
            ExportFormat = "ZIP"
        });

        _context.Consoles.Add(new RomPilot.Core.Models.Console
        {
            Id = 2,
            Name = "Super Nintendo Entertainment System",
            ShortName = "snes",
            RecalboxFolderName = "snes",
            SupportedFormats = "[\".smc\", \".sfc\"]",
            ExportFormat = "ZIP"
        });

        _context.Consoles.Add(new RomPilot.Core.Models.Console
        {
            Id = 3,
            Name = "Sony Playstation",
            ShortName = "psx",
            RecalboxFolderName = "psx",
            SupportedFormats = "[\".iso\", \".bin\", \".cue\"]",
            ExportFormat = "ZIP"
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task US1_Scan7ZArchive_ShouldExtractROMsFrom7Z()
    {
        // Arrange - Utiliser le scénario simple qui contient games.7z
        var simpleScenario = TestDataHelper.GetScenarioPath("simple");
        var archive7zPath = Path.Combine(simpleScenario, "archives", "games.7z");

        // Vérifier que l'archive existe
        File.Exists(archive7zPath).Should().BeTrue("because 7Z archive should be generated in simple scenario");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { simpleScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier qu'on trouve le fichier dans l'archive 7Z
        var fileIn7z = resultsList.FirstOrDefault(f =>
            f.FilePath.Contains("games.7z") &&
            f.FileName.Contains("game_7z"));

        fileIn7z.Should().NotBeNull("because 7Z archive should contain ROMs");
        fileIn7z!.ArchivePath.Should().Be(archive7zPath);
        fileIn7z.ArchiveDepth.Should().Be(1);
        fileIn7z.FilePathInArchive.Should().NotBeNullOrEmpty();
        fileIn7z.IdentificationStatus.Should().NotBe("Failed", "because extraction from 7Z should succeed");

        System.Console.WriteLine($"✅ 7Z archive test:");
        System.Console.WriteLine($"  FilePath: {fileIn7z.FilePath}");
        System.Console.WriteLine($"  ArchivePath: {fileIn7z.ArchivePath}");
        System.Console.WriteLine($"  FilePathInArchive: {fileIn7z.FilePathInArchive}");
    }

    [Fact]
    public async Task US1_ScanRARArchive_ShouldExtractROMsFromRAR()
    {
        // Arrange - Utiliser le scénario simple qui contient games.rar
        var simpleScenario = TestDataHelper.GetScenarioPath("simple");
        var archiveRarPath = Path.Combine(simpleScenario, "archives", "games.rar");

        // Vérifier que l'archive existe
        File.Exists(archiveRarPath).Should().BeTrue("because RAR archive should be generated in simple scenario");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { simpleScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier qu'on trouve le fichier dans l'archive RAR
        var fileInRar = resultsList.FirstOrDefault(f =>
            f.FilePath.Contains("games.rar") &&
            f.FileName.Contains("game_rar"));

        fileInRar.Should().NotBeNull("because RAR archive should contain ROMs");
        fileInRar!.ArchivePath.Should().Be(archiveRarPath);
        fileInRar.ArchiveDepth.Should().Be(1);
        fileInRar.FilePathInArchive.Should().NotBeNullOrEmpty();
        fileInRar.IdentificationStatus.Should().NotBe("Failed", "because extraction from RAR should succeed");

        System.Console.WriteLine($"✅ RAR archive test:");
        System.Console.WriteLine($"  FilePath: {fileInRar.FilePath}");
        System.Console.WriteLine($"  ArchivePath: {fileInRar.ArchivePath}");
        System.Console.WriteLine($"  FilePathInArchive: {fileInRar.FilePathInArchive}");
    }

    [Fact]
    public async Task US1_Scan7ZInZIP_ShouldExtractFromNested7Z()
    {
        // Arrange - Utiliser le scénario medium qui contient nested7z.zip (7Z dans ZIP)
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");
        var nested7zZipPath = Path.Combine(mediumScenario, "nested7z.zip");

        File.Exists(nested7zZipPath).Should().BeTrue("because nested7z.zip should be generated in medium scenario");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { mediumScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier qu'on trouve le fichier dans l'archive 7Z imbriquée dans ZIP
        var fileInNested7z = resultsList.FirstOrDefault(f =>
            f.FilePath.Contains("nested7z.zip") &&
            f.FilePath.Contains("inner.7z") &&
            f.FileName.Contains("nested7z_game"));

        fileInNested7z.Should().NotBeNull("because nested 7Z in ZIP should be processed");
        fileInNested7z!.ArchivePath.Should().Be(nested7zZipPath);
        fileInNested7z.ArchiveDepth.Should().BeGreaterThan(1, "because file is in nested archive");
        fileInNested7z.FilePathInArchive.Should().Contain("inner.7z", "because FilePathInArchive should contain nested archive path");
        fileInNested7z.IdentificationStatus.Should().NotBe("Failed", "because extraction from nested 7Z should succeed");

        System.Console.WriteLine($"✅ 7Z in ZIP test:");
        System.Console.WriteLine($"  FilePath: {fileInNested7z.FilePath}");
        System.Console.WriteLine($"  ArchivePath: {fileInNested7z.ArchivePath}");
        System.Console.WriteLine($"  FilePathInArchive: {fileInNested7z.FilePathInArchive}");
        System.Console.WriteLine($"  ArchiveDepth: {fileInNested7z.ArchiveDepth}");
    }

    [Fact]
    public async Task US1_ScanRARIn7Z_ShouldExtractFromNestedRAR()
    {
        // Arrange - Utiliser le scénario medium qui contient nestedRar.7z (RAR dans 7Z)
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");
        var nestedRar7zPath = Path.Combine(mediumScenario, "nestedRar.7z");

        File.Exists(nestedRar7zPath).Should().BeTrue("because nestedRar.7z should be generated in medium scenario");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { mediumScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier qu'on trouve le fichier dans l'archive RAR imbriquée dans 7Z
        var fileInNestedRar = resultsList.FirstOrDefault(f =>
            f.FilePath.Contains("nestedRar.7z") &&
            f.FilePath.Contains("inner.rar") &&
            f.FileName.Contains("nestedRar_game"));

        fileInNestedRar.Should().NotBeNull("because nested RAR in 7Z should be processed");
        fileInNestedRar!.ArchivePath.Should().Be(nestedRar7zPath);
        fileInNestedRar.ArchiveDepth.Should().BeGreaterThan(1, "because file is in nested archive");
        fileInNestedRar.FilePathInArchive.Should().Contain("inner.rar", "because FilePathInArchive should contain nested archive path");
        fileInNestedRar.IdentificationStatus.Should().NotBe("Failed", "because extraction from nested RAR should succeed");

        System.Console.WriteLine($"✅ RAR in 7Z test:");
        System.Console.WriteLine($"  FilePath: {fileInNestedRar.FilePath}");
        System.Console.WriteLine($"  ArchivePath: {fileInNestedRar.ArchivePath}");
        System.Console.WriteLine($"  FilePathInArchive: {fileInNestedRar.FilePathInArchive}");
        System.Console.WriteLine($"  ArchiveDepth: {fileInNestedRar.ArchiveDepth}");
    }

    [Fact]
    public async Task US1_ScanDeepNestedMixedFormats_ShouldExtractFrom3Levels()
    {
        // Arrange - Utiliser le scénario medium qui contient deep.rar (ZIP dans 7Z dans RAR)
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");
        var deepRarPath = Path.Combine(mediumScenario, "deep.rar");

        File.Exists(deepRarPath).Should().BeTrue("because deep.rar should be generated in medium scenario");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { mediumScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier qu'on trouve le fichier dans l'archive à 3 niveaux (ZIP dans 7Z dans RAR)
        var deepNestedFile = resultsList.FirstOrDefault(f =>
            f.FilePath.Contains("deep.rar") &&
            f.FilePath.Contains("level2.7z") &&
            f.FilePath.Contains("level3.zip") &&
            f.FileName.Contains("deep_game"));

        deepNestedFile.Should().NotBeNull("because deep nested archive (ZIP in 7Z in RAR) should be processed");
        deepNestedFile!.ArchivePath.Should().Be(deepRarPath);
        deepNestedFile.ArchiveDepth.Should().BeGreaterThanOrEqualTo(3, "because file is in 3 levels of nested archives");
        deepNestedFile.FilePathInArchive.Should().Contain("level2.7z", "because FilePathInArchive should contain nested archive paths");
        deepNestedFile.FilePathInArchive.Should().Contain("level3.zip", "because FilePathInArchive should contain all nested archive levels");
        deepNestedFile.IdentificationStatus.Should().NotBe("Failed", "because extraction from deep nested archives should succeed");

        // Vérifier que les checksums ont été calculés
        var checksums = await _context.Checksums
            .Where(c => c.ScannedFileId == deepNestedFile.Id)
            .ToListAsync();
        checksums.Should().NotBeEmpty("because checksums should be calculated for deep nested files");

        System.Console.WriteLine($"✅ Deep nested mixed formats test:");
        System.Console.WriteLine($"  FilePath: {deepNestedFile.FilePath}");
        System.Console.WriteLine($"  ArchivePath: {deepNestedFile.ArchivePath}");
        System.Console.WriteLine($"  FilePathInArchive: {deepNestedFile.FilePathInArchive}");
        System.Console.WriteLine($"  ArchiveDepth: {deepNestedFile.ArchiveDepth}");
        System.Console.WriteLine($"  Checksums calculated: {checksums.Count}");
    }

    [Fact]
    public async Task US1_ExtractFileFrom7Z_ShouldWork()
    {
        // Arrange
        var archiveScanner = new ArchiveScanner();
        var simpleScenario = TestDataHelper.GetScenarioPath("simple");
        var archive7zPath = Path.Combine(simpleScenario, "archives", "games.7z");

        File.Exists(archive7zPath).Should().BeTrue();

        // Act & Assert - Vérifier qu'on peut extraire depuis 7Z
        var filePathInArchive = "game_7z.smc";
        var stream = await archiveScanner.ExtractFileAsync(archive7zPath, filePathInArchive);

        stream.Should().NotBeNull("because extraction from 7Z should succeed");
        stream.Length.Should().BeGreaterThan(0, "because extracted file should have content");

        stream.Dispose();
    }

    [Fact]
    public async Task US1_ExtractFileFromRAR_ShouldWork()
    {
        // Arrange
        var archiveScanner = new ArchiveScanner();
        var simpleScenario = TestDataHelper.GetScenarioPath("simple");
        var archiveRarPath = Path.Combine(simpleScenario, "archives", "games.rar");

        File.Exists(archiveRarPath).Should().BeTrue();

        // Act & Assert - Vérifier qu'on peut extraire depuis RAR
        var filePathInArchive = "game_rar.iso";
        var stream = await archiveScanner.ExtractFileAsync(archiveRarPath, filePathInArchive);

        stream.Should().NotBeNull("because extraction from RAR should succeed");
        stream.Length.Should().BeGreaterThan(0, "because extracted file should have content");

        stream.Dispose();
    }

    [Fact]
    public async Task US1_ExtractFileFromNested7ZInZIP_ShouldWork()
    {
        // Arrange
        var archiveScanner = new ArchiveScanner();
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");
        var nested7zZipPath = Path.Combine(mediumScenario, "nested7z.zip");

        File.Exists(nested7zZipPath).Should().BeTrue();

        // Act & Assert - Vérifier qu'on peut extraire depuis 7Z imbriqué dans ZIP
        // FilePathInArchive devrait être: "inner.7z/nested7z_game.smc"
        var filePathInArchive = "inner.7z/nested7z_game.smc";
        var stream = await archiveScanner.ExtractFileAsync(nested7zZipPath, filePathInArchive);

        stream.Should().NotBeNull("because extraction from nested 7Z in ZIP should succeed");
        stream.Length.Should().BeGreaterThan(0, "because extracted file should have content");

        stream.Dispose();
    }

    [Fact]
    public async Task US1_ScanAllArchiveFormats_ShouldFindAllROMs()
    {
        // Arrange - Utiliser le scénario medium qui contient ZIP, 7Z, RAR et formats mixtes
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { mediumScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier qu'on trouve des fichiers dans tous les formats
        var zipFiles = resultsList.Where(f => f.ArchivePath != null && f.ArchivePath.EndsWith(".zip")).ToList();
        var sevenZipFiles = resultsList.Where(f => f.ArchivePath != null && f.ArchivePath.EndsWith(".7z")).ToList();
        var rarFiles = resultsList.Where(f => f.ArchivePath != null && f.ArchivePath.EndsWith(".rar")).ToList();

        zipFiles.Should().NotBeEmpty("because ZIP archives should be processed");
        sevenZipFiles.Should().NotBeEmpty("because 7Z archives should be processed");
        rarFiles.Should().NotBeEmpty("because RAR archives should be processed");

        // Vérifier qu'aucun fichier n'a échoué à cause du format d'archive
        var failedDueToFormat = resultsList.Where(f =>
            f.IdentificationStatus == "Failed" &&
            f.FailureReason != null &&
            (f.FailureReason.Contains("archive") || f.FailureReason.Contains("format"))).ToList();

        failedDueToFormat.Should().BeEmpty("because all archive formats should be supported");

        System.Console.WriteLine($"✅ All archive formats test:");
        System.Console.WriteLine($"  ZIP files: {zipFiles.Count}");
        System.Console.WriteLine($"  7Z files: {sevenZipFiles.Count}");
        System.Console.WriteLine($"  RAR files: {rarFiles.Count}");
        System.Console.WriteLine($"  Total files in archives: {resultsList.Count(f => f.ArchivePath != null)}");
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
