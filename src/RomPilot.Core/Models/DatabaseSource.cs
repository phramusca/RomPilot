namespace RomPilot.Core.Models;

/// <summary>
/// Represents a database source (NoIntro, Redump, GoodSet).
/// </summary>
public class DatabaseSource
{
    public int Id { get; set; }

    /// <summary>
    /// Name of the database source (e.g., "NoIntro", "Redump", "GoodSet")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Path to the local datfile
    /// </summary>
    public string? DatfilePath { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<GameEntry> GameEntries { get; set; } = new List<GameEntry>();
}

