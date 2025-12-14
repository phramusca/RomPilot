using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using FluentAssertions;
using RomPilot.Core.Archives;
using RomPilot.Tests.Helpers;
using Xunit;

namespace RomPilot.Tests.Unit.Archives;

/// <summary>
/// Unit tests for ArchiveScanner RAR support.
/// Tests that the scanner correctly detects and processes RAR archives.
/// </summary>
public class ArchiveScannerRarTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ArchiveScanner _scanner;

    public ArchiveScannerRarTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _scanner = new ArchiveScanner();
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldDetectRARFiles()
    {
        // Arrange - Créer une vraie archive RAR avec un fichier ROM
        var romFile = Path.Combine(_testDirectory, "game.nes");
        await File.WriteAllBytesAsync(romFile, new byte[] { 0x4E, 0x45, 0x53, 0x1A, 0x01, 0x02, 0x03, 0x04 });

        var rarFile = Path.Combine(_testDirectory, "archive.rar");
        CreateRarArchive(rarFile, new[] { romFile });

        // Act
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null);

        // Assert
        results.Should().NotBeNull();
        // Le scanner devrait reconnaître le fichier RAR comme archive
        var rarResult = results.FirstOrDefault(r => r.FilePath.Contains("archive.rar"));
        rarResult.Should().NotBeNull("because RAR files should be detected as archives");
    }

    [Fact]
    public async Task ScanDirectoryAsync_ShouldExtractROMsFromRAR()
    {
        // Arrange - Créer une archive RAR avec un fichier ROM
        var romFile = Path.Combine(_testDirectory, "game.nes");
        await File.WriteAllBytesAsync(romFile, new byte[] { 0x4E, 0x45, 0x53, 0x1A, 0x01, 0x02, 0x03, 0x04 });

        var rarFile = Path.Combine(_testDirectory, "roms.rar");
        CreateRarArchive(rarFile, new[] { romFile });

        // Act
        var results = await _scanner.ScanDirectoryAsync(_testDirectory, 5, null);

        // Assert
        results.Should().NotBeNull();
        // Le scanner devrait extraire le fichier ROM de l'archive RAR
        var romInRar = results.FirstOrDefault(r =>
            r.FilePath.Contains("roms.rar") &&
            Path.GetFileName(r.FilePath) == "game.nes");

        romInRar.Should().NotBeNull("because ROMs should be extracted from RAR archives");
        romInRar!.ArchivePath.Should().Be(rarFile);
        romInRar.ArchiveDepth.Should().Be(1);
    }

    [Fact]
    public async Task ExtractFileAsync_ShouldExtractFromRAR()
    {
        // Arrange
        var romFile = Path.Combine(_testDirectory, "game.nes");
        var romContent = new byte[] { 0x4E, 0x45, 0x53, 0x1A, 0x01, 0x02, 0x03, 0x04 };
        await File.WriteAllBytesAsync(romFile, romContent);

        var rarFile = Path.Combine(_testDirectory, "archive.rar");
        CreateRarArchive(rarFile, new[] { romFile });

        // Act
        var stream = await _scanner.ExtractFileAsync(rarFile, "game.nes");

        // Assert
        stream.Should().NotBeNull("because extraction from RAR should succeed");
        stream.Length.Should().Be(romContent.Length, "because extracted content should match original");

        var extractedContent = new byte[stream.Length];
        stream.Read(extractedContent, 0, extractedContent.Length);
        extractedContent.Should().BeEquivalentTo(romContent, "because extracted content should match original file");

        stream.Dispose();
    }

    private void CreateRarArchive(string archivePath, string[] files)
    {
        // unrar ne peut PAS créer des archives, seulement les extraire
        // Il faut utiliser 'rar' (non-libre) pour créer des archives
        // Si 'rar' n'est pas disponible, on skip le test
        if (!CommandExists("rar"))
        {
            // unrar ne peut pas créer d'archives, seulement les extraire
            // Si rar n'est pas disponible, on ne peut pas créer d'archives RAR
            // Le test échouera avec un message clair
            throw new InvalidOperationException("rar command not found. Please install rar (non-free) to create RAR archives. unrar can only extract, not create.");
        }

        var fileList = string.Join(" ", files.Where(File.Exists).Select(f => $"\"{f}\""));
        var arguments = $"a -ep \"{archivePath}\" {fileList}";

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "rar",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });
        process?.WaitForExit();

        if (process?.ExitCode != 0 || !File.Exists(archivePath))
        {
            var error = process?.StandardError?.ReadToEnd() ?? process?.StandardOutput?.ReadToEnd() ?? "Unknown error";
            Assert.True(false, $"rar command failed: {error}");
        }
    }

    private bool CommandExists(string command)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}




