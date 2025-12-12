namespace RomPilot.Core.Services;

/// <summary>
/// Service pour gérer les filtres d'exclusion lors du scan.
/// </summary>
public interface IFilterService
{
    /// <summary>
    /// Initialise les filtres par défaut si absents de la base de données.
    /// </summary>
    Task InitializeDefaultFiltersAsync();
    
    /// <summary>
    /// Vérifie si un fichier doit être exclu selon les filtres actifs.
    /// </summary>
    /// <param name="filePath">Chemin du fichier</param>
    /// <param name="fileSize">Taille du fichier en octets</param>
    /// <returns>Tuple (shouldExclude, reason) - true si le fichier doit être exclu, avec la raison</returns>
    Task<(bool shouldExclude, string? reason)> ShouldExcludeFileAsync(string filePath, long fileSize);
    
    /// <summary>
    /// Obtient la liste des extensions exclues par défaut.
    /// </summary>
    Task<IEnumerable<string>> GetDefaultExcludedExtensionsAsync();
    
    /// <summary>
    /// Obtient tous les filtres actifs.
    /// </summary>
    Task<IEnumerable<Models.ExclusionFilter>> GetActiveFiltersAsync();
    
    /// <summary>
    /// Ajoute un filtre d'exclusion personnalisé.
    /// </summary>
    Task AddCustomFilterAsync(string filterType, string filterValue, string? description = null);
    
    /// <summary>
    /// Active ou désactive un filtre.
    /// </summary>
    Task SetFilterActiveAsync(int filterId, bool isActive);
    
    /// <summary>
    /// Supprime un filtre personnalisé.
    /// </summary>
    Task DeleteCustomFilterAsync(int filterId);
}

