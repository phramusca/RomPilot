namespace RomPilot.Core.Models;

/// <summary>
/// Represents a gaming console or platform.
/// </summary>
public class Console
{
    public int Id { get; set; }
    
    /// <summary>
    /// Full name of the console (e.g., "Nintendo Entertainment System")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Short name identifier (e.g., "nes")
    /// </summary>
    public string ShortName { get; set; } = string.Empty;
    
    /// <summary>
    /// Folder name used by Recalbox
    /// </summary>
    public string RecalboxFolderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Platform ID used by Romm (if applicable)
    /// </summary>
    public string? RommPlatformId { get; set; }
    
    /// <summary>
    /// Supported file formats (JSON array)
    /// </summary>
    public string? SupportedFormats { get; set; }
    
    /// <summary>
    /// Export format requirement (ZIP or uncompressed)
    /// </summary>
    public string? ExportFormat { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ScannedFile> ScannedFiles { get; set; } = new List<ScannedFile>();
    public ICollection<Game> Games { get; set; } = new List<Game>();
    public ICollection<GameEntry> GameEntries { get; set; } = new List<GameEntry>();
}

