namespace RomPilot.Core.Models;

/// <summary>
/// Représente une configuration d'export vers une plateforme (Recalbox ou Romm).
/// Supporte export local (dossier) et distant (SSH/SFTP).
/// </summary>
public class ExportConfiguration
{
    public int Id { get; set; }
    
    /// <summary>
    /// Nom de la configuration (ex: "Recalbox Production", "Romm Dev")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Plateforme cible: "Recalbox" ou "Romm"
    /// </summary>
    public string Platform { get; set; } = string.Empty;
    
    /// <summary>
    /// Type d'export: "Local" ou "Remote"
    /// </summary>
    public string ExportType { get; set; } = "Local";
    
    // Configuration export local
    /// <summary>
    /// Chemin local de destination (si ExportType = "Local")
    /// </summary>
    public string? LocalPath { get; set; }
    
    // Configuration export distant SSH/SFTP
    /// <summary>
    /// Hôte SSH/SFTP (si ExportType = "Remote")
    /// </summary>
    public string? SftpHost { get; set; }
    
    /// <summary>
    /// Port SSH/SFTP (défaut: 22)
    /// </summary>
    public int SftpPort { get; set; } = 22;
    
    /// <summary>
    /// Nom d'utilisateur SSH/SFTP
    /// </summary>
    public string? SftpUsername { get; set; }
    
    /// <summary>
    /// Méthode d'authentification: "Password" ou "Key"
    /// </summary>
    public string? SftpAuthMethod { get; set; }
    
    /// <summary>
    /// Mot de passe chiffré (si SftpAuthMethod = "Password")
    /// </summary>
    public string? SftpPasswordEncrypted { get; set; }
    
    /// <summary>
    /// Chemin vers clé privée SSH (si SftpAuthMethod = "Key")
    /// </summary>
    public string? SftpKeyPath { get; set; }
    
    /// <summary>
    /// Répertoire distant sur serveur SSH/SFTP
    /// </summary>
    public string? RemoteDirectory { get; set; }
    
    // Options export
    /// <summary>
    /// Format requis: "ZIP" ou "Uncompressed"
    /// </summary>
    public string? FormatRequirement { get; set; }
    
    /// <summary>
    /// Résolution conflits: "Overwrite", "Skip", "Ask"
    /// </summary>
    public string ConflictResolution { get; set; } = "Ask";
    
    // Status
    /// <summary>
    /// Configuration active/utilisée
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Dernier test de connexion (si Remote)
    /// </summary>
    public DateTime? LastConnectionTest { get; set; }
    
    /// <summary>
    /// Résultat du test: "Success", "Failed", null
    /// </summary>
    public string? ConnectionTestStatus { get; set; }
    
    /// <summary>
    /// Message d'erreur du dernier test de connexion
    /// </summary>
    public string? ConnectionTestError { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

