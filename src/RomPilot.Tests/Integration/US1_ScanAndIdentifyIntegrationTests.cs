using System;
using System.IO;
using System.IO.Compression;
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
/// Tests d'intégration pour US1 : Scanner et Identifier
/// Teste le workflow complet avec filtres d'exclusion, scan Quick/Full, et compteurs
/// </summary>
public class US1_ScanAndIdentifyIntegrationTests : IDisposable
{
    private readonly RomPilotDbContext _context;
    private readonly IScanService _scanService;
    private readonly IFilterService _filterService;
    private readonly string _testDirectory;
    private readonly string _tempTestDirectory; // Répertoire temporaire pour les fichiers créés par les tests
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

        // Use test data directory instead of temporary directory
        _testDirectory = TestDataHelper.GetScenarioPath("simple");

        // Créer un répertoire temporaire unique pour les fichiers créés par les tests
        _tempTestDirectory = Path.Combine(Path.GetTempPath(), $"RomPilotTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempTestDirectory);
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

        // Assert - Vérifier les compteurs exacts
        // Scénario simple devrait contenir :
        // - 3 ROMs directes (game1.nes, game2.smc, game3.iso)
        // - 3 fichiers exclus (cover.jpg, readme.txt, manual.pdf)
        // - 3 ROMs dans archives (games.zip/game_zip.nes, games.7z/game_7z.smc, games.rar/game_rar.iso)
        // Total attendu : 9 fichiers (sans répertoires sources)
        // Si les répertoires sources existent, il y aura 3 fichiers de plus (doublons)

        var excluded = resultsList.Count(f => f.IdentificationStatus == "Excluded");
        var unidentified = resultsList.Count(f => f.IdentificationStatus == "Unidentified");

        // Vérifier le nombre exact de fichiers exclus (3 fichiers : .jpg, .txt, .pdf)
        excluded.Should().Be(3, "because exactly 3 files (.jpg, .txt, .pdf) should be excluded in simple scenario");

        // Vérifier qu'il n'y a pas de doublons (fichiers sources non supprimés)
        // C'est le point crucial : si les répertoires sources existent, il y aura des fichiers dans archives/ avec ArchivePath == null
        var sourceFilesInArchivesDir = resultsList.Where(f => f.FilePath.Contains("/archives/") && f.ArchivePath == null).ToList();
        sourceFilesInArchivesDir.Should().BeEmpty("because source directories should be deleted after archive creation - no direct files in archives/ directory");

        // Vérifier le nombre de fichiers non identifiés
        // Scénario simple devrait avoir : 3 ROMs directes + 3 ROMs dans archives = 6 ROMs
        // Mais test.nes peut être créé par d'autres tests, donc on vérifie >= 6
        // Si les répertoires sources existent, il y aura 3 fichiers de plus (doublons)
        unidentified.Should().BeGreaterThanOrEqualTo(6, "because at least 6 ROMs (3 direct + 3 in archives) should be scanned but not identified (no database)");

        // Si les répertoires sources existent, il y aura 3 fichiers de plus (les ROMs sources dans archives/)
        // On vérifie qu'il n'y a pas trop de fichiers (indiquant des doublons)
        // Maximum attendu : 6 ROMs de base + 1 test.nes + 1 music.mp3 (créés par d'autres tests) = 8
        // Si on trouve 9 ou plus, c'est qu'il y a des doublons (répertoires sources non supprimés)
        if (sourceFilesInArchivesDir.Count > 0)
        {
            // Si des fichiers sources existent, on devrait avoir 3 fichiers de plus
            unidentified.Should().BeGreaterThanOrEqualTo(11, "because with source directories, there should be at least 11 unidentified files (6 base + 2 from other tests + 3 duplicates)");
        }
        else
        {
            // Sans fichiers sources, maximum 8 (6 ROMs + test.nes + music.mp3 créés par d'autres tests)
            unidentified.Should().BeLessThanOrEqualTo(8, "because without source directories, there should be at most 8 unidentified files (6 ROMs + test.nes + music.mp3 from other tests)");
        }

        // Vérifier que les fichiers exclus ont une raison
        var excludedFiles = resultsList.Where(f => f.IdentificationStatus == "Excluded").ToList();
        excludedFiles.Should().AllSatisfy(f => f.ExclusionReason.Should().NotBeNullOrEmpty());

        // Vérifier le contenu de la base de données
        var dbFiles = await _context.ScannedFiles.ToListAsync();
        dbFiles.Count.Should().Be(resultsList.Count, "because database should contain exactly the same number of files as scan results");
    }

    [Fact]
    public async Task US1_QuickScan_ShouldReuseChecksumsForUnchangedFiles()
    {
        // Arrange - Utiliser le répertoire temporaire pour éviter de polluer test-data
        var testFile = Path.Combine(_tempTestDirectory, "test.nes");
        await File.WriteAllBytesAsync(testFile, new byte[] { 0x4E, 0x45, 0x53, 0x1A }); // NES header

        // First scan (Full) - Scanner le répertoire temporaire
        var firstScan = await _scanService.ScanDirectoriesAsync(
            new[] { _tempTestDirectory },
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
            new[] { _tempTestDirectory },
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
        // Arrange - Utiliser le répertoire temporaire
        var testFile = Path.Combine(_tempTestDirectory, "test.nes");
        await File.WriteAllBytesAsync(testFile, new byte[] { 0x4E, 0x45, 0x53, 0x1A });

        // First scan
        await _scanService.ScanDirectoriesAsync(
            new[] { _tempTestDirectory },
            ScanType.Quick,
            progressReporter: _progressReporter);

        // Act - Full scan
        var fullScan = await _scanService.ScanDirectoriesAsync(
            new[] { _tempTestDirectory },
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
        // Arrange - Utiliser le répertoire temporaire
        var mp3File = Path.Combine(_tempTestDirectory, "music.mp3");
        await File.WriteAllTextAsync(mp3File, "fake mp3 content");

        // First scan without mp3 filter
        var firstScan = await _scanService.ScanDirectoriesAsync(
            new[] { _tempTestDirectory },
            progressReporter: _progressReporter);

        var firstMp3 = firstScan.FirstOrDefault(f => f.FileName == "music.mp3");
        firstMp3?.IdentificationStatus.Should().NotBe("Excluded", "because .mp3 is not in default filters");

        // Act - Add custom filter for .mp3
        await _filterService.AddCustomFilterAsync("Extension", ".mp3", "Fichiers audio MP3");

        // Second scan with mp3 filter
        var secondScan = await _scanService.ScanDirectoriesAsync(
            new[] { _tempTestDirectory },
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

        // Assert - Calculer les compteurs exacts
        var identified = resultsList.Count(f => f.IdentificationStatus == "Identified");
        var unidentified = resultsList.Count(f => f.IdentificationStatus == "Unidentified");
        var excluded = resultsList.Count(f => f.IdentificationStatus == "Excluded");
        var failed = resultsList.Count(f => f.IdentificationStatus == "Failed");

        // Vérifier que tous les fichiers ont un statut
        (identified + unidentified + excluded + failed).Should().Be(resultsList.Count);

        // Vérifier les nombres exacts pour le scénario simple
        excluded.Should().Be(3, "because exactly 3 files should be excluded in simple scenario");
        identified.Should().Be(0, "because no ROMs should be identified without database");
        failed.Should().Be(0, "because no files should fail in simple scenario");

        // Vérifier qu'il n'y a pas de doublons (fichiers sources non supprimés)
        // C'est le point crucial : si les répertoires sources existent, il y aura des fichiers dans archives/ avec ArchivePath == null
        var sourceFilesInArchivesDir = resultsList.Where(f => f.FilePath.Contains("/archives/") && f.ArchivePath == null).ToList();
        sourceFilesInArchivesDir.Should().BeEmpty("because source directories should be deleted after archive creation");

        // Vérifier le nombre de fichiers non identifiés
        // Scénario simple devrait avoir : 3 ROMs directes + 3 ROMs dans archives = 6 ROMs
        // Mais test.nes peut être créé par d'autres tests, donc on vérifie >= 6
        // Si les répertoires sources existent, il y aura 3 fichiers de plus (doublons)
        unidentified.Should().BeGreaterThanOrEqualTo(6, "because at least 6 ROMs (3 direct + 3 in archives) should be scanned but not identified (no database)");

        if (sourceFilesInArchivesDir.Count > 0)
        {
            // Si des fichiers sources existent, on devrait avoir 3 fichiers de plus
            unidentified.Should().BeGreaterThanOrEqualTo(11, "because with source directories, there should be at least 11 unidentified files (6 base + 2 from other tests + 3 duplicates)");
        }
        else
        {
            // Sans fichiers sources, maximum 8 (6 ROMs + test.nes + music.mp3 créés par d'autres tests)
            unidentified.Should().BeLessThanOrEqualTo(8, "because without source directories, there should be at most 8 unidentified files (6 ROMs + test.nes + music.mp3 from other tests)");
        }

        // Log pour debug
        System.Console.WriteLine($"Status counts - Identified: {identified}, Unidentified: {unidentified}, Excluded: {excluded}, Failed: {failed}");
    }

    private void CreateTestFiles()
    {
        // Les fichiers sont déjà générés par TestDataHelper
        // Cette méthode est conservée pour compatibilité mais n'est plus nécessaire
    }

    [Fact]
    public async Task US1_ScanNestedArchives_ShouldStoreCorrectFilePathInArchive()
    {
        // Arrange - Utiliser le scénario medium qui contient des archives imbriquées
        var mediumScenario = TestDataHelper.GetScenarioPath("medium");

        // Act
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { mediumScenario },
            progressReporter: _progressReporter);

        var resultsList = results.ToList();

        // Debug: afficher tous les fichiers trouvés
        System.Console.WriteLine($"\n📊 Total files found: {resultsList.Count}");
        foreach (var f in resultsList)
        {
            if (f.FilePath.Contains("nested") || !string.IsNullOrEmpty(f.ArchivePath))
            {
                System.Console.WriteLine($"  - {f.FilePath} | ArchivePath: {f.ArchivePath ?? "null"} | Depth: {f.ArchiveDepth} | InArchive: {f.FilePathInArchive} | Status: {f.IdentificationStatus}");
            }
        }

        // Assert - Vérifier que les fichiers dans archives imbriquées ont FilePath correct
        // Le FilePath devrait contenir le chemin virtuel complet: archive.zip/nested.zip/file.nes
        var filesInArchives = resultsList.Where(f => !string.IsNullOrEmpty(f.ArchivePath)).ToList();
        var nestedFiles = filesInArchives.Where(f =>
            f.FilePath.Contains(".zip/") &&
            f.FilePath.Split('/').Count(part => part.EndsWith(".zip")) > 1).ToList();

        System.Console.WriteLine($"\n📦 Files in archives: {filesInArchives.Count}");
        System.Console.WriteLine($"📦 Files in nested archives: {nestedFiles.Count}");
        foreach (var f in nestedFiles)
        {
            System.Console.WriteLine($"  - {f.FilePath}");
        }

        nestedFiles.Should().NotBeEmpty("because medium scenario contains nested archives");

        // Vérifier qu'au moins un fichier dans archive imbriquée a FilePath avec chemin complet
        var fileInNestedArchive = nestedFiles.FirstOrDefault(f =>
            f.FilePath.Contains(".zip/") &&
            f.FilePath.Split('/').Count(part => part.EndsWith(".zip")) > 1);

        fileInNestedArchive.Should().NotBeNull("because nested archives should have FilePath with nested archive path");

        // Vérifier le format du FilePath: doit contenir archive.zip/nested.zip/file.nes
        fileInNestedArchive!.FilePath.Should().Contain(".zip/", "because nested archive path should contain .zip/");
        fileInNestedArchive.FilePath.Should().MatchRegex(@".*\.zip/.*\.zip/.*", "because nested archive should have format: archive.zip/nested.zip/file.nes");

        // Vérifier que ArchivePath pointe vers l'archive source
        fileInNestedArchive.ArchivePath.Should().NotBeNullOrEmpty();
        fileInNestedArchive.ArchivePath.Should().EndWith(".zip", "because ArchivePath should point to source archive");

        // Vérifier que FilePathInArchive contient le chemin relatif dans l'archive
        fileInNestedArchive.FilePathInArchive.Should().Contain("/", "because nested archive should have FilePathInArchive with separator");

        System.Console.WriteLine($"Found nested file:");
        System.Console.WriteLine($"  FilePath: {fileInNestedArchive.FilePath}");
        System.Console.WriteLine($"  ArchivePath: {fileInNestedArchive.ArchivePath}");
        System.Console.WriteLine($"  FilePathInArchive: {fileInNestedArchive.FilePathInArchive}");
        System.Console.WriteLine($"  ArchiveDepth: {fileInNestedArchive.ArchiveDepth}");
    }

    [Fact]
    public async Task US1_ScanLoadScenario_ShouldHandleManyFiles()
    {
        // Arrange - Utiliser le scénario de charge
        var loadScenario = TestDataHelper.GetScenarioPath("load");

        // Act
        var startTime = DateTime.Now;
        var results = await _scanService.ScanDirectoriesAsync(
            new[] { loadScenario },
            progressReporter: _progressReporter);
        var duration = DateTime.Now - startTime;

        var resultsList = results.ToList();

        // Assert
        // Scénario load devrait contenir :
        // - 100 ROMs directes
        // - 100 fichiers exclus
        // - 200 ROMs dans archives (10 archives × 20 ROMs)
        // - 25 ROMs dans archives imbriquées (5 archives × 5 ROMs)
        // Total attendu : 425 fichiers (sans répertoires sources)
        // Si les répertoires sources existent, il y aura beaucoup plus de fichiers (doublons)

        resultsList.Count.Should().Be(425, "because load scenario should have exactly 425 files (100 ROMs + 100 excluded + 200 in archives + 25 in nested) without source directories");

        // Vérifier les différents statuts
        var identified = resultsList.Count(f => f.IdentificationStatus == "Identified");
        var unidentified = resultsList.Count(f => f.IdentificationStatus == "Unidentified");
        var excluded = resultsList.Count(f => f.IdentificationStatus == "Excluded");

        excluded.Should().Be(100, "because load scenario should have exactly 100 excluded files");
        unidentified.Should().Be(325, "because load scenario should have exactly 325 ROMs (100 direct + 200 in archives + 25 in nested) without database");

        // Vérifier qu'il n'y a pas de doublons (fichiers sources non supprimés)
        var filesInArchiveDirs = resultsList.Where(f =>
            (f.FilePath.Contains("/archive") && f.ArchivePath == null) ||
            (f.FilePath.Contains("/nested") && f.ArchivePath == null && !f.FilePath.Contains(".zip") && !f.FilePath.Contains(".7z") && !f.FilePath.Contains(".rar"))
        ).ToList();
        filesInArchiveDirs.Should().BeEmpty("because source directories should be deleted after archive creation - no direct files in archive*/ or nested*/ directories");

        System.Console.WriteLine($"Load scenario scan completed in {duration.TotalSeconds:F2} seconds");
        System.Console.WriteLine($"Files: {resultsList.Count} total, {identified} identified, {unidentified} unidentified, {excluded} excluded");
    }

    [Fact]
    public async Task US1_ScanSourceAndArchiveFiles_ShouldTreatAsDistinctButDetectDuplicates()
    {
        // Arrange - Créer un scénario avec un fichier source ET le même fichier dans une archive
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Créer un fichier ROM source
            var sourceRomPath = Path.Combine(tempDir, "source_game.nes");
            var romContent = new byte[] { 0x4E, 0x45, 0x53, 0x1A }; // NES header
            await File.WriteAllBytesAsync(sourceRomPath, romContent);

            // Créer une archive contenant le même fichier
            var archiveDir = Path.Combine(tempDir, "archive_source");
            Directory.CreateDirectory(archiveDir);
            var romInArchivePath = Path.Combine(archiveDir, "source_game.nes");
            await File.WriteAllBytesAsync(romInArchivePath, romContent);
            var archivePath = Path.Combine(tempDir, "archive.zip");
            ZipFile.CreateFromDirectory(archiveDir, archivePath);
            // Supprimer le répertoire source après création de l'archive
            Directory.Delete(archiveDir, recursive: true);

            // Act
            var results = await _scanService.ScanDirectoriesAsync(
                new[] { tempDir },
                ScanType.Full,
                progressReporter: _progressReporter);

            var resultsList = results.ToList();

            // Assert - Les deux fichiers doivent être traités comme distincts
            var sourceFile = resultsList.FirstOrDefault(f => f.FilePath == sourceRomPath && f.ArchivePath == null);
            var archiveFile = resultsList.FirstOrDefault(f => f.FilePath.Contains("archive.zip") && f.ArchivePath == archivePath);

            sourceFile.Should().NotBeNull("because source file should be scanned");
            archiveFile.Should().NotBeNull("because file in archive should be scanned");

            // Vérifier qu'ils sont bien distincts (IDs différents)
            sourceFile!.Id.Should().NotBe(archiveFile!.Id, "because source file and archive file should be distinct entries");

            // Vérifier qu'ils ont le même checksum (donc sont des doublons)
            var sourceChecksums = await _context.Checksums
                .Where(c => c.ScannedFileId == sourceFile.Id)
                .ToListAsync();
            var archiveChecksums = await _context.Checksums
                .Where(c => c.ScannedFileId == archiveFile.Id)
                .ToListAsync();

            sourceChecksums.Should().NotBeEmpty("because source file should have checksums");
            archiveChecksums.Should().NotBeEmpty("because archive file should have checksums");

            // Vérifier qu'au moins un checksum (MD5) est identique
            var sourceMD5 = sourceChecksums.FirstOrDefault(c => c.HashType == "MD5");
            var archiveMD5 = archiveChecksums.FirstOrDefault(c => c.HashType == "MD5");

            sourceMD5.Should().NotBeNull("because source file should have MD5 checksum");
            archiveMD5.Should().NotBeNull("because archive file should have MD5 checksum");
            sourceMD5!.HashValue.Should().Be(archiveMD5!.HashValue, "because same file content should produce same MD5 checksum");

            // Vérifier que les checksums sont bien stockés dans la base de données pour les deux fichiers
            var checksumRepository = new ChecksumRepository(_context);
            var allFilesWithSameMD5 = await checksumRepository.GetAllByHashAsync("MD5", sourceMD5.HashValue);
            var filesWithSameMD5 = allFilesWithSameMD5.ToList();

            filesWithSameMD5.Should().HaveCountGreaterThanOrEqualTo(2, "because both source and archive files should have the same MD5 checksum stored");
            filesWithSameMD5.Should().Contain(c => c.ScannedFileId == sourceFile.Id, "because source file checksum should be stored");
            filesWithSameMD5.Should().Contain(c => c.ScannedFileId == archiveFile.Id, "because archive file checksum should be stored");

            System.Console.WriteLine($"✅ Duplicate detection test:");
            System.Console.WriteLine($"  Source file ID: {sourceFile.Id}, Path: {sourceFile.FilePath}");
            System.Console.WriteLine($"  Archive file ID: {archiveFile.Id}, Path: {archiveFile.FilePath}");
            System.Console.WriteLine($"  MD5 checksum (both): {sourceMD5.HashValue}");
            System.Console.WriteLine($"  Files with same MD5: {filesWithSameMD5.Count}");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    public void Dispose()
    {
        // Cleanup
        _context.Database.CloseConnection();
        _context.Dispose();

        // Nettoyer le répertoire temporaire créé pour les tests
        if (Directory.Exists(_tempTestDirectory))
        {
            Directory.Delete(_tempTestDirectory, recursive: true);
        }

        // Ne pas supprimer _testDirectory car c'est maintenant test-data/simple qui est partagé
    }
}

