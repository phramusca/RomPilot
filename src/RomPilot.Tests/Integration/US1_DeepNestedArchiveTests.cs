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
/// Tests pour vérifier que les archives profondément imbriquées (3+ niveaux) fonctionnent correctement
/// </summary>
public class US1_DeepNestedArchiveTests : IDisposable
{
    private readonly RomPilotDbContext _context;
    private readonly IScanService _scanService;
    private readonly IFilterService _filterService;
    private readonly string _testDirectory;
    private readonly ScanProgressReporter _progressReporter;

    public US1_DeepNestedArchiveTests()
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
        _testDirectory = TestDataHelper.GetScenarioPath("medium");
    }

    private void SeedTestData()
    {
        // Add test console
        _context.Consoles.Add(new RomPilot.Core.Models.Console
        {
            Id = 1,
            Name = "Nintendo Entertainment System",
            ShortName = "nes",
            RecalboxFolderName = "nes",
            SupportedFormats = "[\".nes\"]",
            ExportFormat = "ZIP"
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task US1_ScanDeepNestedArchive_ShouldExtractFileFrom3Levels()
    {
        // Arrange - Utiliser le scénario medium qui contient deep.rar avec 3 niveaux (ZIP dans 7Z dans RAR)
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { mediumScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier qu'on trouve le fichier dans l'archive à 3 niveaux
        // Format: deep.rar (RAR) contient level2.7z (7Z) qui contient level3.zip (ZIP) qui contient deep_game.gba
        var deepNestedFile = resultsList.FirstOrDefault(f =>
            f.FilePath.Contains("deep.rar") &&
            f.FilePath.Contains("level2.7z") &&
            f.FilePath.Contains("level3.zip"));

        deepNestedFile.Should().NotBeNull("because medium scenario contains deep.rar with 3 levels of nesting (ZIP in 7Z in RAR)");

        // Vérifier le format du FilePath: doit contenir les 3 niveaux
        deepNestedFile!.FilePath.Should().MatchRegex(@".*deep\.rar/.*level2\.7z/.*level3\.zip/.*",
            "because deep nested archive should have format: deep.rar/level2.7z/level3.zip/file.gba");

        // Vérifier que FilePathInArchive contient le chemin relatif complet
        deepNestedFile.FilePathInArchive.Should().Contain("level2.7z/level3.zip/",
            "because FilePathInArchive should contain all nested archive levels");

        // Vérifier qu'on peut extraire le fichier (calcul des checksums doit réussir)
        deepNestedFile.IdentificationStatus.Should().NotBe("Failed",
            "because checksum calculation should succeed for deep nested files");

        // Vérifier que les checksums ont été calculés
        var checksums = await _context.Checksums
            .Where(c => c.ScannedFileId == deepNestedFile.Id)
            .ToListAsync();
        checksums.Should().NotBeEmpty("because checksums should be calculated for deep nested files");

        System.Console.WriteLine($"✅ Deep nested file found:");
        System.Console.WriteLine($"  FilePath: {deepNestedFile.FilePath}");
        System.Console.WriteLine($"  FilePathInArchive: {deepNestedFile.FilePathInArchive}");
        System.Console.WriteLine($"  Checksums calculated: {checksums.Count}");
    }

    [Fact]
    public async Task US1_ExtractFileFromDeepNestedArchive_ShouldWork()
    {
        // Arrange
        var archiveScanner = new ArchiveScanner();
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");
        var deepRarPath = Path.Combine(mediumScenario, "deep.rar");

        // Act & Assert - Vérifier qu'on peut extraire depuis 3 niveaux
        // Format: deep.rar (RAR) contient level2.7z (7Z) qui contient level3.zip (ZIP) qui contient deep_game.gba
        // FilePathInArchive devrait être: "level2.7z/level3.zip/deep_game.gba"
        var stream = await archiveScanner.ExtractFileAsync(deepRarPath, "level2.7z/level3.zip/deep_game.gba");

        stream.Should().NotBeNull("because extraction should succeed");
        stream.Length.Should().BeGreaterThan(0, "because extracted file should have content");

        // Vérifier le contenu (header GBA)
        var buffer = new byte[4];
        stream.Read(buffer, 0, 4);
        buffer.Should().BeEquivalentTo(new byte[] { 0x24, 0xFF, 0xAE, 0x51 },
            "because extracted file should have GBA header");

        stream.Dispose();
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}

