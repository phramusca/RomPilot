using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;

namespace RomPilot.Core.Archives;

/// <summary>
/// Implementation of archive scanner for ZIP and 7Z archives with recursive support.
/// </summary>
public class ArchiveScanner : IArchiveScanner
{
    private readonly HashSet<string> _romExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".nes", ".snes", ".smc", ".gb", ".gbc", ".gba", ".n64", ".z64", ".v64",
        ".md", ".gen", ".sms", ".gg", ".pce", ".ngp", ".ngc", ".ws", ".wsc",
        ".psx", ".bin", ".cue", ".iso", ".img", ".mdf", ".chd",
        ".nds", ".dsi", ".3ds", ".cia", ".cci",
        ".gcz", ".wbfs", ".wad", ".rvz",
        ".rom", ".sfc", ".smd", ".32x", ".a26", ".lynx", ".jag"
    };

    public async Task<IEnumerable<ArchiveFileInfo>> ScanDirectoryAsync(
        string directoryPath,
        int maxDepth = 5,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ArchiveFileInfo>();
        await ScanDirectoryRecursiveAsync(directoryPath, maxDepth, 0, results, cancellationToken);
        return results;
    }

    private async Task ScanDirectoryRecursiveAsync(
        string path,
        int maxDepth,
        int currentDepth,
        List<ArchiveFileInfo> results,
        CancellationToken cancellationToken)
    {
        if (currentDepth > maxDepth || cancellationToken.IsCancellationRequested)
            return;

        if (!Directory.Exists(path))
            return;

        try
        {
            // Scan direct files
            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var extension = Path.GetExtension(file);
                if (_romExtensions.Contains(extension))
                {
                    var fileInfo = new FileInfo(file);
                    results.Add(new ArchiveFileInfo
                    {
                        FilePath = file,
                        ArchivePath = null,
                        ArchiveDepth = 0,
                        FileSize = fileInfo.Length
                    });
                }
                else if (IsArchiveFile(file) && currentDepth < maxDepth)
                {
                    // Scan archive contents
                    await ScanArchiveAsync(file, currentDepth + 1, maxDepth, results, cancellationToken);
                }
            }

            // Scan subdirectories
            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ScanDirectoryRecursiveAsync(dir, maxDepth, currentDepth, results, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log error but continue scanning
            System.Diagnostics.Debug.WriteLine($"Error scanning {path}: {ex.Message}");
        }
    }

    private async Task ScanArchiveAsync(
        string archivePath,
        int currentDepth,
        int maxDepth,
        List<ArchiveFileInfo> results,
        CancellationToken cancellationToken)
    {
        try
        {
            IArchive? archive = null;
            
            if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                archive = ZipArchive.Open(archivePath);
            }
            else if (archivePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
            {
                archive = SevenZipArchive.Open(archivePath);
            }

            if (archive == null)
                return;

            await Task.Run(() =>
            {
                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var extension = Path.GetExtension(entry.Key);
                    if (!string.IsNullOrEmpty(extension) && _romExtensions.Contains(extension))
                    {
                        results.Add(new ArchiveFileInfo
                        {
                            FilePath = entry.Key,
                            ArchivePath = archivePath,
                            ArchiveDepth = currentDepth,
                            FileSize = entry.Size
                        });
                    }
                    else if (!string.IsNullOrEmpty(entry.Key) && IsArchiveFile(entry.Key) && currentDepth < maxDepth)
                    {
                        // Nested archive - extract and scan recursively
                        using var entryStream = entry.OpenEntryStream();
                        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(entry.Key));
                        try
                        {
                            using var fileStream = File.Create(tempPath);
                            entryStream.CopyTo(fileStream);
                            ScanArchiveAsync(tempPath, currentDepth + 1, maxDepth, results, cancellationToken).Wait(cancellationToken);
                        }
                        finally
                        {
                            if (File.Exists(tempPath))
                                File.Delete(tempPath);
                        }
                    }
                }
            }, cancellationToken);

            archive.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scanning archive {archivePath}: {ex.Message}");
        }
    }

    public async Task<Stream> ExtractFileAsync(string archivePath, string filePathInArchive)
    {
        return await Task.Run(() =>
        {
            IArchive? archive = null;
            
            if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                archive = ZipArchive.Open(archivePath);
            }
            else if (archivePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
            {
                archive = SevenZipArchive.Open(archivePath);
            }

            if (archive == null)
                throw new InvalidOperationException($"Unsupported archive format: {archivePath}");

            var entry = archive.Entries.FirstOrDefault(e => e.Key == filePathInArchive);
            if (entry == null)
                throw new FileNotFoundException($"File {filePathInArchive} not found in archive {archivePath}");

            var memoryStream = new MemoryStream();
            using (var entryStream = entry.OpenEntryStream())
            {
                entryStream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            
            archive.Dispose();
            return memoryStream;
        });
    }

    private static bool IsArchiveFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".7z", StringComparison.OrdinalIgnoreCase);
    }
}

