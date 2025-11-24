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
    /// <summary>
    /// Path to the file within the archive (relative path)
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Path to the original source archive (the archive that was initially scanned)
    /// </summary>
    public string? ArchivePath { get; set; }
    
    /// <summary>
    /// Path to the immediate archive containing this file (for extraction purposes).
    /// For nested archives, this is the archive that directly contains the file.
    /// For top-level archives, this is the same as ArchivePath.
    /// </summary>
    public string? ImmediateArchivePath { get; set; }
    
    public int ArchiveDepth { get; set; }
    public long FileSize { get; set; }
}

