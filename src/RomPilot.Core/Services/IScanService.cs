using RomPilot.Core.Models;

namespace RomPilot.Core.Services;

/// <summary>
/// Interface for scanning directories and identifying ROM files.
/// </summary>
public interface IScanService
{
    /// <summary>
    /// Scans one or more directories for files, calculates checksums, and identifies ROMs.
    /// </summary>
    /// <param name="directoryPaths">Paths to directories to scan</param>
    /// <param name="progressReporter">Optional progress reporter for UI updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of scanned files</returns>
    Task<IEnumerable<ScannedFile>> ScanDirectoriesAsync(
        IEnumerable<string> directoryPaths,
        IScanProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);
}

