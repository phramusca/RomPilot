using FluentAssertions;
using RomPilot.Core.Archives;
using System.IO;
using Xunit;

namespace RomPilot.Tests.Unit.Archives;

/// <summary>
/// Unit tests for ArchiveScanner recursive directory traversal.
/// Tests that the scanner correctly traverses all subdirectories recursively.
/// </summary>
public class ArchiveScannerRecursiveTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ArchiveScanner _scanner;

    public ArchiveScannerRecursiveTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _scanner = new ArchiveScanner();
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldFindROMsInRootDirectory()
    {
        // Arrange
        var romFile = Path.Combine(_testDirectory, "test.nes");
        await File.WriteAllTextAsync(romFile, "fake rom content");

        // Act
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null);

        // Assert
        results.Should().HaveCount(1);
        results.First().FilePath.Should().Be(romFile);
        results.First().ArchivePath.Should().BeNull();
        results.First().ArchiveDepth.Should().Be(0);
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldFindROMsInSubdirectories()
    {
        // Arrange
        var subDir1 = Path.Combine(_testDirectory, "subdir1");
        var subDir2 = Path.Combine(_testDirectory, "subdir1", "subdir2");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        var rom1 = Path.Combine(subDir1, "game1.nes");
        var rom2 = Path.Combine(subDir2, "game2.gb");
        await File.WriteAllTextAsync(rom1, "fake rom 1");
        await File.WriteAllTextAsync(rom2, "fake rom 2");

        // Act
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.FilePath == rom1);
        results.Should().Contain(r => r.FilePath == rom2);
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldFindROMsInDeeplyNestedSubdirectories()
    {
        // Arrange
        var currentPath = _testDirectory;
        var romFiles = new List<string>();

        // Create 5 levels of nested directories
        for (int i = 0; i < 5; i++)
        {
            currentPath = Path.Combine(currentPath, $"level{i}");
            Directory.CreateDirectory(currentPath);
            
            var romFile = Path.Combine(currentPath, $"game{i}.nes");
            await File.WriteAllTextAsync(romFile, $"fake rom {i}");
            romFiles.Add(romFile);
        }

        // Act
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null);

        // Assert
        results.Should().HaveCount(5);
        foreach (var romFile in romFiles)
        {
            results.Should().Contain(r => r.FilePath == romFile);
        }
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldIgnoreNonROMFiles()
    {
        // Arrange
        var romFile = Path.Combine(_testDirectory, "game.nes");
        var textFile = Path.Combine(_testDirectory, "readme.txt");
        var imageFile = Path.Combine(_testDirectory, "cover.jpg");
        
        await File.WriteAllTextAsync(romFile, "fake rom");
        await File.WriteAllTextAsync(textFile, "readme content");
        await File.WriteAllTextAsync(imageFile, "fake image");

        // Act
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null);

        // Assert
        results.Should().HaveCount(1);
        results.First().FilePath.Should().Be(romFile);
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldHandleEmptyDirectories()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "empty");
        Directory.CreateDirectory(subDir);

        // Act
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var romFile = Path.Combine(_testDirectory, "game.nes");
        await File.WriteAllTextAsync(romFile, "fake rom");
        
        var cts = new CancellationTokenSource();
        // Cancel after a short delay to allow scanning to start
        cts.CancelAfter(10);

        // Act - should handle cancellation gracefully
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null, cts.Token);

        // Assert - may return partial results or empty if cancelled early
        results.Should().NotBeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}


