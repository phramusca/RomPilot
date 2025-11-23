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
}

