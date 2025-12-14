namespace RomPilot.Core.Models;

/// <summary>
/// Représente un fichier scanné (peut être identifié comme ROM ou non).
/// CHANGEMENT MAJEUR: Renommé de RomFile à ScannedFile pour refléter que tous les fichiers
/// sont scannés sans présupposition sur ce qui est une ROM.
/// </summary>
public class ScannedFile
{
    public int Id { get; set; }

    /// <summary>
    /// Chemin virtuel complet du fichier.
    /// Pour les fichiers directs: chemin réel sur le disque.
    /// Pour les fichiers dans archives: chemin virtuel incluant les archives imbriquées.
    /// 
    /// Exemples:
    /// - Fichier direct: "/workspace/test-data/medium/game1.nes"
    /// - Fichier dans archive: "/workspace/test-data/medium/archive.zip/game.nes"
    /// - Fichier dans archive imbriquée: "/workspace/test-data/medium/nested.zip/inner.zip/nested_game.nes"
    /// 
    /// Ce format permet:
    /// - Comparaison facile pour quick scan (timestamp/size)
    /// - Extraction facile (parser le chemin pour extraire depuis archives imbriquées)
    /// - Identification claire de l'emplacement du fichier
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Nom du fichier uniquement
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Taille du fichier en octets
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Timestamp de dernière modification (Unix timestamp pour scans incrémentaux)
    /// </summary>
    public long LastModifiedTimestamp { get; set; }

    /// <summary>
    /// Chemin de l'archive source si le fichier est dans une archive ZIP/7Z/RAR
    /// </summary>
    public string? ArchivePath { get; set; }

    /// <summary>
    /// Chemin complet du fichier dans l'archive source, incluant les archives imbriquées.
    /// Exemple: "archive2.zip/game.nes" (si game.nes est dans archive2.zip qui est dans archive1.zip)
    /// Exemple: "game.nes" (si fichier directement dans archive source)
    /// Vide si fichier n'est pas dans une archive.
    /// </summary>
    public string FilePathInArchive { get; set; } = string.Empty;

    /// <summary>
    /// Profondeur dans les archives imbriquées (0 = fichier direct)
    /// </summary>
    public int ArchiveDepth { get; set; }

    /// <summary>
    /// Statut d'identification: "Identified", "Unidentified", "Excluded", "Failed"
    /// </summary>
    public string IdentificationStatus { get; set; } = "Unidentified";

    /// <summary>
    /// Raison d'exclusion si IdentificationStatus = "Excluded"
    /// (ex: "Extension .jpg exclue par filtre par défaut")
    /// </summary>
    public string? ExclusionReason { get; set; }

    /// <summary>
    /// Raison d'échec si IdentificationStatus = "Failed"
    /// (ex: "Archive corrompue", "Erreur calcul checksum")
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Foreign key vers Console (NULL si fichier non identifié comme ROM)
    /// </summary>
    public int? ConsoleId { get; set; }

    /// <summary>
    /// Foreign key vers GameEntry (NULL si fichier non identifié)
    /// </summary>
    public int? GameEntryId { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date du dernier scan de ce fichier
    /// </summary>
    public DateTime? LastScannedAt { get; set; }

    /// <summary>
    /// Type du dernier scan: "Quick" (rapide) ou "Full" (complet)
    /// </summary>
    public string? ScanType { get; set; }

    // Navigation properties
    public Console? Console { get; set; }
    public GameEntry? GameEntry { get; set; }
    public ICollection<Checksum> Checksums { get; set; } = new List<Checksum>();
    public ICollection<GameRomVersion> GameRomVersions { get; set; } = new List<GameRomVersion>();
}

