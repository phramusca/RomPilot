namespace RomPilot.Core.Models;

/// <summary>
/// Association between a game and its ROM versions.
/// </summary>
public class GameRomVersion
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to Game
    /// </summary>
    public int GameId { get; set; }
    
    /// <summary>
    /// Foreign key to RomFile
    /// </summary>
    public int RomFileId { get; set; }
    
    /// <summary>
    /// Foreign key to GameEntry (if identified in database)
    /// </summary>
    public int? GameEntryId { get; set; }
    
    /// <summary>
    /// Whether this version is selected
    /// </summary>
    public bool IsSelected { get; set; }
    
    /// <summary>
    /// Selection score for sorting
    /// </summary>
    public int? SelectionScore { get; set; }
    
    // Navigation properties
    public Game Game { get; set; } = null!;
    public RomFile RomFile { get; set; } = null!;
    public GameEntry? GameEntry { get; set; }
}

