namespace RomPilot.Core.Models;

/// <summary>
/// User data for games (favorites, statistics, etc.).
/// </summary>
public class UserData
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Game
    /// </summary>
    public int GameId { get; set; }

    public bool IsFavorite { get; set; }
    public bool IsHidden { get; set; }

    /// <summary>
    /// User rating
    /// </summary>
    public double? UserRating { get; set; }

    public int PlayCount { get; set; }
    public DateTime? LastPlayedAt { get; set; }

    /// <summary>
    /// Total play time in seconds
    /// </summary>
    public int TimePlayed { get; set; }

    public string? Notes { get; set; }

    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Game Game { get; set; } = null!;
}

