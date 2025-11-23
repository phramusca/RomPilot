namespace RomPilot.Core.Models;

/// <summary>
/// Represents a single ROM file found during scanning.
/// </summary>
public class RomFile
{
    public int Id { get; set; }
    
    /// <summary>
    /// Full path to the ROM file
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// File name only
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Path to archive if ROM is inside ZIP/7Z
    /// </summary>
    public string? ArchivePath { get; set; }
    
    /// <summary>
    /// Depth level in nested archives (0 = direct file)
    /// </summary>
    public int ArchiveDepth { get; set; }
    
    /// <summary>
    /// Foreign key to Console
    /// </summary>
    public int ConsoleId { get; set; }
    
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastScannedAt { get; set; }
    
    // Navigation properties
    public Console Console { get; set; } = null!;
    public ICollection<Checksum> Checksums { get; set; } = new List<Checksum>();
    public ICollection<GameRomVersion> GameRomVersions { get; set; } = new List<GameRomVersion>();
}

