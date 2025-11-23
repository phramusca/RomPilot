namespace RomPilot.Core.Services;

/// <summary>
/// Interface for detecting console/platform from ROM file.
/// </summary>
public interface IConsoleDetectionService
{
    /// <summary>
    /// Detects the console/platform for a ROM file based on file extension, header, or other heuristics.
    /// </summary>
    /// <param name="filePath">Path to the ROM file</param>
    /// <param name="fileSize">Size of the file in bytes</param>
    /// <param name="fileStream">Optional file stream for header analysis</param>
    /// <returns>Console short name if detected, null otherwise</returns>
    Task<string?> DetectConsoleAsync(string filePath, long fileSize, Stream? fileStream = null);
}

