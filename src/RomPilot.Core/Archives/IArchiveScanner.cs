namespace RomPilot.Core.Archives;

/// <summary>
/// Interface for scanning archives (ZIP, 7Z) recursively to find ROM files.
/// </summary>
public interface IArchiveScanner
{
    /// <summary>
    /// Scans a directory recursively for ROM files, including those in archives.
    /// </summary>
    /// <param name="directoryPath">Path to scan</param>
    /// <param name="maxDepth">Maximum depth for nested archives (default: 5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of found ROM file paths with their archive locations</returns>
    Task<IEnumerable<ArchiveFileInfo>> ScanDirectoryAsync(
        string directoryPath, 
        int maxDepth = 5, 
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
    public string FilePath { get; set; } = string.Empty;
    public string? ArchivePath { get; set; }
    public int ArchiveDepth { get; set; }
    public long FileSize { get; set; }
}

