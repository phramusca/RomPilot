namespace RomPilot.Core.Models;

/// <summary>
/// Représente une base de données de référence téléchargée (NoIntro, Redump, GoodSet).
/// Une version spécifique pour une console donnée.
/// </summary>
public class ReferenceDatabase
{
    public int Id { get; set; }

    /// <summary>
    /// Provider de la base de données: "NoIntro", "Redump", "GoodSet"
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Console/plateforme (ex: "nes", "snes", "psx")
    /// </summary>
    public string Console { get; set; } = string.Empty;

    /// <summary>
    /// Version du datfile (ex: "2024-01-15", "v2023-12")
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Date de publication de cette version
    /// </summary>
    public string? ReleaseDate { get; set; }

    /// <summary>
    /// Taille du fichier en octets
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Statut du téléchargement: "Available", "Downloading", "Downloaded", "Error"
    /// </summary>
    public string DownloadStatus { get; set; } = "Available";

    /// <summary>
    /// Chemin du fichier local si téléchargé
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// True si cette version est celle par défaut pour cette console
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Date de téléchargement réussie
    /// </summary>
    public DateTime? DownloadedAt { get; set; }

    /// <summary>
    /// Dernière vérification de disponibilité de mise à jour
    /// </summary>
    public DateTime? LastCheckedAt { get; set; }

    /// <summary>
    /// Message d'erreur si DownloadStatus = "Error"
    /// </summary>
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<GameEntry> GameEntries { get; set; } = new List<GameEntry>();
}

