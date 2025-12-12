namespace RomPilot.Core.Models;

/// <summary>
/// Represents a game entry from a reference database (NoIntro, Redump, GoodSet).
/// </summary>
public class GameEntry
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to ReferenceDatabase (changé de DatabaseSourceId)
    /// </summary>
    public int ReferenceDatabaseId { get; set; }
    
    /// <summary>
    /// Name of the game
    /// </summary>
    public string GameName { get; set; } = string.Empty;
    
    /// <summary>
    /// Foreign key to Console
    /// </summary>
    public int ConsoleId { get; set; }
    
    /// <summary>
    /// Hash type used for identification (e.g., "MD5", "SHA1")
    /// </summary>
    public string HashType { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash value for matching
    /// </summary>
    public string HashValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Region (e.g., "EUR", "USA", "JAP")
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// Video format: "PAL", "NTSC", "NTSC-J" (NOUVEAU - séparé de région)
    /// </summary>
    public string? VideoFormat { get; set; }
    
    /// <summary>
    /// Language (e.g., "FR", "EN", "JP")
    /// </summary>
    public string? Language { get; set; }
    
    /// <summary>
    /// Quality flags (JSON array: ["bad dump", ...])
    /// </summary>
    public string? QualityFlags { get; set; }
    
    /// <summary>
    /// Version of the ROM
    /// </summary>
    public string? Version { get; set; }
    
    /// <summary>
    /// Serial number (if applicable)
    /// </summary>
    public string? SerialNumber { get; set; }
    
    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }
    
    // Navigation properties
    public ReferenceDatabase ReferenceDatabase { get; set; } = null!;
    public Console Console { get; set; } = null!;
    public ICollection<GameRomVersion> GameRomVersions { get; set; } = new List<GameRomVersion>();
}

