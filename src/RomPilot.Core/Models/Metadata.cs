namespace RomPilot.Core.Models;

/// <summary>
/// Game metadata from various sources (scraped or user-entered).
/// </summary>
public class Metadata
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Game
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// Source: "Recalbox", "Romm", "User", "Scraped"
    /// </summary>
    public string Source { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Rating (0.0 - 10.0)
    /// </summary>
    public double? Rating { get; set; }

    public string? ReleaseDate { get; set; }
    public string? Developer { get; set; }
    public string? Publisher { get; set; }
    public string? Genre { get; set; }

    public string? CoverImagePath { get; set; }
    public string? ThumbnailPath { get; set; }

    /// <summary>
    /// Screenshot paths (JSON array)
    /// </summary>
    public string? ScreenshotPaths { get; set; }

    public string? VideoPath { get; set; }

    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Game Game { get; set; } = null!;
}

