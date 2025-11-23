namespace RomPilot.Core.Models;

/// <summary>
/// Represents a logical game that may have multiple ROM versions.
/// </summary>
public class Game
{
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the game
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Foreign key to Console
    /// </summary>
    public int ConsoleId { get; set; }
    
    /// <summary>
    /// Selected database source ID (when multiple sources available)
    /// </summary>
    public int? SelectedDatabaseSourceId { get; set; }
    
    /// <summary>
    /// Selected ROM file version ID
    /// </summary>
    public int? SelectedRomFileId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Console Console { get; set; } = null!;
    public DatabaseSource? SelectedDatabaseSource { get; set; }
    public RomFile? SelectedRomFile { get; set; }
    public ICollection<GameRomVersion> GameRomVersions { get; set; } = new List<GameRomVersion>();
    public ICollection<Metadata> Metadata { get; set; } = new List<Metadata>();
    public UserData? UserData { get; set; }
}

