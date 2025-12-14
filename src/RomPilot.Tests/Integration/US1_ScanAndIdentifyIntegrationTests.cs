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
using Xunit;

namespace RomPilot.Tests.Integration;

/// <summary>
/// Tests d'intégration pour US1 : Scanner et Identifier
/// Teste le workflow complet avec filtres d'exclusion, scan Quick/Full, et compteurs
/// </summary>
public class US1_ScanAndIdentifyIntegrationTests : IDisposable
{
    private readonly RomPilotDbContext _context;
    private readonly IScanService _scanService;
    private readonly IFilterService _filterService;
    private readonly string _testDirectory;
    private readonly ScanProgressReporter _progressReporter;

    public US1_ScanAndIdentifyIntegrationTests()
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

        // Create test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), "RomPilotTests_US1_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
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
    public async Task US1_ScanWithExclusionFilters_ShouldExcludeMatchingFiles()
    {
        // Arrange
        CreateTestFiles();

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Vérifier les compteurs
        var excluded = resultsList.Count(f => f.IdentificationStatus == "Excluded");
        var unidentified = resultsList.Count(f => f.IdentificationStatus == "Unidentified");

        excluded.Should().BeGreaterThanOrEqualTo(3, "because .jpg, .txt, and .pdf files should be excluded");
        unidentified.Should().BeGreaterThanOrEqualTo(1, "because .nes file should be scanned but not identified (no database)");

        // Vérifier que les fichiers exclus ont une raison
        var excludedFiles = resultsList.Where(f => f.IdentificationStatus == "Excluded").ToList();
        excludedFiles.Should().AllSatisfy(f => f.ExclusionReason.Should().NotBeNullOrEmpty());

        // Vérifier le contenu de la base de données
        var dbFiles = await _context.ScannedFiles.ToListAsync();
        dbFiles.Count.Should().BeGreaterThanOrEqualTo(resultsList.Count);
    }

    [Fact]
    public async Task US1_QuickScan_ShouldReuseChecksumsForUnchangedFiles()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.nes");
        await File.WriteAllBytesAsync(testFile, new byte[] { 0x4E, 0x45, 0x53, 0x1A }); // NES header

        // First scan (Full)
        var firstScan = await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            ScanType.Full,
            progressReporter: _progressReporter);

        var firstFile = firstScan.First(f => f.FileName == "test.nes");
        var firstTimestamp = firstFile.LastModifiedTimestamp;

        // Vérifier que les checksums ont été calculés
        var checksums = await _context.Checksums
            .Where(c => c.ScannedFileId == firstFile.Id)
            .ToListAsync();
        checksums.Should().NotBeEmpty("because checksums should be calculated in Full scan");

        // Act - Second scan (Quick) sans modifier le fichier
        var secondScan = await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            ScanType.Quick,
            progressReporter: _progressReporter);

        var secondFile = secondScan.First(f => f.FileName == "test.nes");

        // Assert
        secondFile.LastModifiedTimestamp.Should().Be(firstTimestamp);
        secondFile.ScanType.Should().Be("Quick");

        // Les checksums devraient être réutilisés (même nombre)
        var secondChecksums = await _context.Checksums
            .Where(c => c.ScannedFileId == secondFile.Id)
            .ToListAsync();
        secondChecksums.Count.Should().Be(checksums.Count);
    }

    [Fact]
    public async Task US1_FullScan_ShouldRecalculateAllChecksums()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "test.nes");
        await File.WriteAllBytesAsync(testFile, new byte[] { 0x4E, 0x45, 0x53, 0x1A });

        // First scan
        await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            ScanType.Quick,
            progressReporter: _progressReporter);

        // Act - Full scan
        var fullScan = await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            ScanType.Full,
            progressReporter: _progressReporter);

        var scannedFile = fullScan.First(f => f.FileName == "test.nes");

        // Assert
        scannedFile.ScanType.Should().Be("Full");

        // Checksums should have been recalculated
        var checksums = await _context.Checksums
            .Where(c => c.ScannedFileId == scannedFile.Id)
            .ToListAsync();
        checksums.Should().NotBeEmpty();
    }

    [Fact]
    public async Task US1_AddCustomFilter_ShouldExcludeNewFileType()
    {
        // Arrange
        var mp3File = Path.Combine(_testDirectory, "music.mp3");
        await File.WriteAllTextAsync(mp3File, "fake mp3 content");

        // First scan without mp3 filter
        var firstScan = await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            progressReporter: _progressReporter);

        var firstMp3 = firstScan.FirstOrDefault(f => f.FileName == "music.mp3");
        firstMp3?.IdentificationStatus.Should().NotBe("Excluded", "because .mp3 is not in default filters");

        // Act - Add custom filter for .mp3
        await _filterService.AddCustomFilterAsync("Extension", ".mp3", "Fichiers audio MP3");

        // Second scan with mp3 filter
        var secondScan = await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            progressReporter: _progressReporter);

        var secondMp3 = secondScan.FirstOrDefault(f => f.FileName == "music.mp3");

        // Assert
        secondMp3.Should().NotBeNull();
        secondMp3!.IdentificationStatus.Should().Be("Excluded");
        secondMp3.ExclusionReason.Should().Contain(".mp3");
    }

    [Fact]
    public async Task US1_StatusCounters_ShouldBeAccurate()
    {
        // Arrange
        CreateTestFiles();

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { _testDirectory },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Assert - Calculer les compteurs
        var identified = resultsList.Count(f => f.IdentificationStatus == "Identified");
        var unidentified = resultsList.Count(f => f.IdentificationStatus == "Unidentified");
        var excluded = resultsList.Count(f => f.IdentificationStatus == "Excluded");
        var failed = resultsList.Count(f => f.IdentificationStatus == "Failed");

        // Vérifier que tous les fichiers ont un statut
        (identified + unidentified + excluded + failed).Should().Be(resultsList.Count);

        // Vérifier qu'on a bien des fichiers exclus (images, textes, docs)
        excluded.Should().BeGreaterThan(0);

        // Vérifier qu'on a bien des fichiers non identifiés (ROMs sans base de données)
        unidentified.Should().BeGreaterThan(0);

        // Log pour debug
        System.Console.WriteLine($"Status counts - Identified: {identified}, Unidentified: {unidentified}, Excluded: {excluded}, Failed: {failed}");
    }

    private void CreateTestFiles()
    {
        // Fichiers qui devraient être scannés
        File.WriteAllBytes(Path.Combine(_testDirectory, "game1.nes"), new byte[] { 0x4E, 0x45, 0x53, 0x1A });
        File.WriteAllBytes(Path.Combine(_testDirectory, "game2.nes"), new byte[] { 0x4E, 0x45, 0x53, 0x1A, 0x00, 0x01 });

        // Fichiers qui devraient être exclus (filtres par défaut)
        File.WriteAllText(Path.Combine(_testDirectory, "cover.jpg"), "fake image");
        File.WriteAllText(Path.Combine(_testDirectory, "readme.txt"), "fake readme");
        File.WriteAllText(Path.Combine(_testDirectory, "manual.pdf"), "fake pdf");
        File.WriteAllText(Path.Combine(_testDirectory, "info.nfo"), "fake nfo");
    }

    public void Dispose()
    {
        // Cleanup
        _context.Database.CloseConnection();
        _context.Dispose();

        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}

