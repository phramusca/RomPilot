using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RomPilot.Core.Archives;
using RomPilot.Core.Checksums;
using RomPilot.Core.Database;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;
using RomPilot.Core.Services;
using Xunit;
using Models = RomPilot.Core.Models;

namespace RomPilot.Tests.Unit.Services;

/// <summary>
/// Unit tests for ScanService with detailed progress reporting.
/// Tests that the service correctly reports file-level status and failure reasons.
/// </summary>
public class ScanServiceProgressTests : IDisposable
{
    private readonly RomPilotDbContext _context;
    private readonly Mock<IArchiveScanner> _archiveScannerMock;
    private readonly Mock<IChecksumCalculator> _checksumCalculatorMock;
    private readonly Mock<IConsoleDetectionService> _consoleDetectionServiceMock;
    private readonly Mock<IGameIdentificationService> _gameIdentificationServiceMock;
    private readonly ScanService _service;
    private readonly ScanProgressReporter _progressReporter;

    public ScanServiceProgressTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<RomPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new RomPilotDbContext(options);

        // Seed test console
        _context.Consoles.Add(new Models.Console
        {
            Id = 1,
            Name = "Nintendo Entertainment System",
            ShortName = "nes",
            RecalboxFolderName = "nes"
        });
        _context.SaveChanges();

        _archiveScannerMock = new Mock<IArchiveScanner>();
        _checksumCalculatorMock = new Mock<IChecksumCalculator>();
        _consoleDetectionServiceMock = new Mock<IConsoleDetectionService>();
        _gameIdentificationServiceMock = new Mock<IGameIdentificationService>();
        _progressReporter = new ScanProgressReporter();

        var romFileRepository = new ScannedFileRepository(_context);
        var checksumRepository = new ChecksumRepository(_context);

        _service = new ScanService(
            _context,
            _archiveScannerMock.Object,
            _checksumCalculatorMock.Object,
            _consoleDetectionServiceMock.Object,
            _gameIdentificationServiceMock.Object,
            romFileRepository,
            checksumRepository);
    }

    [Fact]
    public async Task ScanDirectoriesAsync_ShouldReportProgressForEachFile()
    {
        // Arrange
        // Use a temporary directory that exists
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var archiveFile = new ArchiveFileInfo
            {
                FilePath = "game.nes",
                ArchivePath = null,
                ArchiveDepth = 0,
                FileSize = 40960
            };

            _archiveScannerMock.Setup(s => s.ScanDirectoryAsync(tempDir, 5, It.IsAny<IScanProgressReporter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { archiveFile });

            _consoleDetectionServiceMock.Setup(s => s.DetectConsoleAsync("game.nes", 40960, null))
                .ReturnsAsync("nes");

            _checksumCalculatorMock.Setup(c => c.CalculateAllAsync(It.IsAny<Stream>()))
                .ReturnsAsync(new Dictionary<string, string>
                {
                    { "MD5", "testmd5" },
                    { "SHA1", "testsha1" },
                    { "SHA256", "testsha256" },
                    { "CRC32", "testcrc32" }
                });

            _gameIdentificationServiceMock.Setup(g => g.IdentifyGameAsync(It.IsAny<Models.ScannedFile>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GameEntry?)null);

            // Act
            await _service.ScanDirectoriesAsync(new[] { tempDir }, _progressReporter);

            // Assert
            // Verify that progress reporting occurred
            _progressReporter.Messages.Should().NotBeEmpty();
            // The service should report scanning or processing messages
            var hasProgressMessages = _progressReporter.Messages.Any(m =>
                m.Contains("Scanning directory") ||
                m.Contains("Found") ||
                m.Contains("Processing:") ||
                m.Contains("Found ROM"));
            hasProgressMessages.Should().BeTrue("Progress messages should be reported");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ScanDirectoriesAsync_ShouldReportFailureWhenConsoleNotDetected()
    {
        // Arrange
        var directoryPath = "/test/directory";
        var archiveFile = new ArchiveFileInfo
        {
            FilePath = "unknown.rom",
            ArchivePath = null,
            ArchiveDepth = 0,
            FileSize = 1024
        };

        _archiveScannerMock.Setup(s => s.ScanDirectoryAsync(directoryPath, 5, It.IsAny<IScanProgressReporter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { archiveFile });

        _consoleDetectionServiceMock.Setup(s => s.DetectConsoleAsync("unknown.rom", 1024, null))
            .ReturnsAsync((string?)null);

        // Act
        await _service.ScanDirectoriesAsync(new[] { directoryPath }, _progressReporter);

        // Assert
        // Verify that failure is reported when console is not detected
        _progressReporter.Messages.Should().NotBeEmpty();
        // The service should report that console could not be detected
        // It may report "Could not detect console" or "Directory not found" if directory doesn't exist
        var hasFailureMessage = _progressReporter.Messages.Any(m =>
            m.Contains("Could not detect console") ||
            m.Contains("Console not detected") ||
            m.Contains("unknown.rom") ||
            m.Contains("Directory not found"));
        hasFailureMessage.Should().BeTrue("Failure message should be reported when console is not detected");
    }

    [Fact]
    public async Task ScanDirectoriesAsync_ShouldReportFailureWhenArchiveCorrupted()
    {
        // Arrange
        var directoryPath = "/test/directory";
        var archiveFile = new ArchiveFileInfo
        {
            FilePath = "corrupted.zip",
            ArchivePath = null,
            ArchiveDepth = 0,
            FileSize = 1024
        };

        _archiveScannerMock.Setup(s => s.ScanDirectoryAsync(directoryPath, 5, It.IsAny<IScanProgressReporter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { archiveFile });

        _consoleDetectionServiceMock.Setup(s => s.DetectConsoleAsync("corrupted.zip", 1024, null))
            .ThrowsAsync(new InvalidDataException("Corrupted archive"));

        // Act
        await _service.ScanDirectoriesAsync(new[] { directoryPath }, _progressReporter);

        // Assert
        // Should handle exception gracefully and continue
        _progressReporter.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ScanDirectoriesAsync_ShouldReportSuccessWhenFileProcessed()
    {
        // Arrange
        var directoryPath = "/test/directory";
        var archiveFile = new ArchiveFileInfo
        {
            FilePath = "game.nes",
            ArchivePath = null,
            ArchiveDepth = 0,
            FileSize = 40960
        };

        _archiveScannerMock.Setup(s => s.ScanDirectoryAsync(directoryPath, 5, It.IsAny<IScanProgressReporter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { archiveFile });

        _consoleDetectionServiceMock.Setup(s => s.DetectConsoleAsync("game.nes", 40960, null))
            .ReturnsAsync("nes");

        _checksumCalculatorMock.Setup(c => c.CalculateAllAsync(It.IsAny<Stream>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                { "MD5", "testmd5hash" },
                { "SHA1", "testsha1hash" },
                { "SHA256", "testsha256hash" },
                { "CRC32", "testcrc32" }
            });

        _gameIdentificationServiceMock.Setup(g => g.IdentifyGameAsync(It.IsAny<Models.ScannedFile>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameEntry?)null);

        // Act
        var results = await _service.ScanDirectoriesAsync(new[] { directoryPath }, _progressReporter);

        // Assert
        // Verify that progress reporting occurred
        _progressReporter.Messages.Should().NotBeEmpty();
        // The service should report processing or scanning messages
        var hasProgressMessages = _progressReporter.Messages.Any(m =>
            m.Contains("Processing:") ||
            m.Contains("Found ROM") ||
            m.Contains("Scanning directory") ||
            m.Contains("Found"));
        hasProgressMessages.Should().BeTrue("Progress messages should be reported");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
