namespace RomPilot.Core.Services;

/// <summary>
/// Interface for reporting scan progress to the UI.
/// </summary>
public interface IScanProgressReporter
{
    /// <summary>
    /// Reports the current progress of the scan operation.
    /// </summary>
    /// <param name="current">Current item being processed</param>
    /// <param name="total">Total items to process</param>
    /// <param name="message">Optional status message</param>
    void ReportProgress(int current, int total, string? message = null);

    /// <summary>
    /// Reports that a file is being processed.
    /// </summary>
    void ReportFileProcessing(string filePath);

    /// <summary>
    /// Reports that a ROM file was found.
    /// </summary>
    void ReportRomFound(string filePath, string consoleName);

    /// <summary>
    /// Reports that a file was excluded by a filter.
    /// </summary>
    /// <param name="filePath">Path to the excluded file</param>
    /// <param name="exclusionReason">Reason for exclusion (e.g., "Extension .jpg excluded by default filter")</param>
    void ReportFileExcluded(string filePath, string exclusionReason);

    /// <summary>
    /// Reports that a file was processed successfully with status details.
    /// </summary>
    /// <param name="filePath">Path to the processed file</param>
    /// <param name="status">Status: "success", "console_identified", "game_identified", etc.</param>
    /// <param name="details">Optional details about the processing result</param>
    void ReportFileSuccess(string filePath, string status, string? details = null);

    /// <summary>
    /// Reports that a file failed to process with explicit failure reason.
    /// </summary>
    /// <param name="filePath">Path to the failed file</param>
    /// <param name="failureReason">Explicit reason for failure (e.g., "Console not detected", "Corrupted archive", "Checksum calculation failed")</param>
    void ReportFileFailure(string filePath, string failureReason);
}

