namespace RomPilot.Core.Models;

/// <summary>
/// Type de scan : rapide (incrémental) ou complet (recalcul total).
/// </summary>
public enum ScanType
{
    /// <summary>
    /// Scan rapide : détecte les changements via timestamp + taille, réutilise checksums existants si inchangé
    /// </summary>
    Quick,

    /// <summary>
    /// Scan complet : recalcule tous les checksums, même pour fichiers inchangés
    /// </summary>
    Full
}

