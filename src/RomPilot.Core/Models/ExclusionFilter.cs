namespace RomPilot.Core.Models;

/// <summary>
/// Représente un filtre d'exclusion pour le scan (extension, taille, pattern).
/// Peut être un filtre par défaut ou personnalisé par l'utilisateur.
/// </summary>
public class ExclusionFilter
{
    public int Id { get; set; }
    
    /// <summary>
    /// Type de filtre: "Extension", "SizeRange", "Pattern"
    /// </summary>
    public string FilterType { get; set; } = string.Empty;
    
    /// <summary>
    /// Valeur du filtre (ex: ".jpg", "0-1KB", "*.tmp")
    /// </summary>
    public string FilterValue { get; set; } = string.Empty;
    
    /// <summary>
    /// True si filtre par défaut (liste initiale de l'application)
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// True si filtre est actif (utilisé lors des scans)
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Description du filtre (ex: "Images JPEG")
    /// </summary>
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

