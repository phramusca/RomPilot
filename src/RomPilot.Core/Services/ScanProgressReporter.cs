using System.Collections.Concurrent;

namespace RomPilot.Core.Services;

/// <summary>
/// Default implementation of scan progress reporter that stores progress in memory.
/// </summary>
public class ScanProgressReporter : IScanProgressReporter
{
    private readonly ConcurrentQueue<string> _messages = new();
    private int _current;
    private int _total;
    private string? _currentMessage;

    public int Current => _current;
    public int Total => _total;
    public string? CurrentMessage => _currentMessage;
    public IEnumerable<string> Messages => _messages;

    public void ReportProgress(int current, int total, string? message = null)
    {
        _current = current;
        _total = total;
        _currentMessage = message;

        if (!string.IsNullOrEmpty(message))
        {
            _messages.Enqueue($"[{current}/{total}] {message}");
        }
    }

    public void ReportFileProcessing(string filePath)
    {
        _messages.Enqueue($"Processing: {filePath}");
    }

    public void ReportRomFound(string filePath, string consoleName)
    {
        _messages.Enqueue($"Found ROM: {Path.GetFileName(filePath)} ({consoleName})");
    }

    public void ReportFileExcluded(string filePath, string exclusionReason)
    {
        _messages.Enqueue($"⊘ {Path.GetFileName(filePath)}: EXCLUDED - {exclusionReason}");
    }

    public void ReportFileSuccess(string filePath, string status, string? details = null)
    {
        var message = $"✓ {Path.GetFileName(filePath)}: {status}";
        if (!string.IsNullOrEmpty(details))
        {
            message += $" ({details})";
        }
        _messages.Enqueue(message);
    }

    public void ReportFileFailure(string filePath, string failureReason)
    {
        _messages.Enqueue($"✗ {Path.GetFileName(filePath)}: FAILED - {failureReason}");
    }

    public void Reset()
    {
        _current = 0;
        _total = 0;
        _currentMessage = null;
        while (_messages.TryDequeue(out _)) { }
    }
}

