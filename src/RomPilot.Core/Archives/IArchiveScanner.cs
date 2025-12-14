using RomPilot.Core.Services;

namespace RomPilot.Core.Archives;

/// <summary>
/// Interface for scanning archives (ZIP, 7Z, RAR) recursively to find ROM files.
/// </summary>
public interface IArchiveScanner
{
    /// <summary>
    /// Scans a directory recursively for ROM files, including those in archives.
    /// </summary>
    /// <param name="directoryPath">Path to scan</param>
    /// <param name="maxDepth">Maximum depth for nested archives (default: 5)</param>
    /// <param name="progressReporter">Optional progress reporter for scanning progress</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of found ROM file paths with their archive locations</returns>
    Task<IEnumerable<ArchiveFileInfo>> ScanDirectoryAsync(
        string directoryPath,
        int maxDepth = 5,
        IScanProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts a file from an archive to a temporary location.
    /// </summary>
    Task<Stream> ExtractFileAsync(string archivePath, string filePathInArchive);
}

/// <summary>
/// Information about a file found in an archive.
/// </summary>
public class ArchiveFileInfo
{
    /// <summary>
    /// Virtual file path representing the complete path from the source archive.
    /// This path includes nested archives and allows easy identification and extraction.
    /// 
    /// Examples:
    /// - Direct file: "/workspace/test-data/medium/game1.nes"
    /// - File in archive: "/workspace/test-data/medium/archive.zip/game.nes"
    /// - File in nested archive: "/workspace/test-data/medium/nested.zip/inner.zip/nested_game.nes"
    /// 
    /// This format allows:
    /// - Easy comparison for quick scan (timestamp/size)
    /// - Easy extraction (parse the path to extract from nested archives)
    /// - Clear identification of file location
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Path to the original source archive (the real file path on disk).
    /// This is the top-level archive that was initially scanned.
    /// Example: "/workspace/test-data/medium/nested.zip"
    /// </summary>
    public string? ArchivePath { get; set; }

    /// <summary>
    /// Path to the immediate archive containing this file (for extraction purposes).
    /// For nested archives, this is the archive that directly contains the file.
    /// For top-level archives, this is the same as ArchivePath.
    /// Example: "/tmp/archive2.zip" (if file is in nested archive) or "/path/to/archive1.zip" (if direct)
    /// </summary>
    public string? ImmediateArchivePath { get; set; }

    /// <summary>
    /// Complete path of the file within the source archive, including nested archive paths.
    /// This is the relative path from ArchivePath.
    /// Example: "inner.zip/nested_game.nes" (if nested_game.nes is in inner.zip which is in nested.zip)
    /// Example: "game.nes" (if file is directly in source archive)
    /// </summary>
    public string FilePathInArchive { get; set; } = string.Empty;

    public int ArchiveDepth { get; set; }
    public long FileSize { get; set; }

    /// <summary>
    /// Last modified timestamp (Unix timestamp)
    /// </summary>
    public long LastModifiedTimestamp { get; set; }
}

