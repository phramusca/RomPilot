using RomPilot.Core.Models;
using RomPilot.Core.Repositories;

namespace RomPilot.Core.Services;

/// <summary>
/// Service pour gérer les filtres d'exclusion lors du scan.
/// </summary>
public class FilterService : IFilterService
{
    private readonly IExclusionFilterRepository _filterRepository;

    // Liste des extensions exclues par défaut (selon specs)
    private static readonly string[] DefaultExcludedExtensions = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".webp", // Images
        ".txt", ".nfo", ".diz", ".md", ".rtf", // Textes
        ".exe", ".dll", ".so", ".dylib", ".bat", ".sh", ".cmd", // Exécutables
        ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".ppt", ".pptx", // Documents
        ".html", ".htm", ".xml", ".json", ".yaml", ".yml", ".css" // Markup/Config
    };

    public FilterService(IExclusionFilterRepository filterRepository)
    {
        _filterRepository = filterRepository;
    }

    public async Task InitializeDefaultFiltersAsync()
    {
        var existingFilters = await _filterRepository.GetDefaultFiltersAsync();
        if (existingFilters.Any())
        {
            return; // Filtres déjà initialisés
        }

        // Créer les filtres par défaut pour chaque extension
        foreach (var extension in DefaultExcludedExtensions)
        {
            var filter = new ExclusionFilter
            {
                FilterType = "Extension",
                FilterValue = extension,
                IsDefault = true,
                IsActive = true,
                Description = $"Extension exclue par défaut : {extension}"
            };

            await _filterRepository.AddAsync(filter);
        }
    }

    public async Task<(bool shouldExclude, string? reason)> ShouldExcludeFileAsync(string filePath, long fileSize)
    {
        var activeFilters = await _filterRepository.GetActiveFiltersAsync();

        foreach (var filter in activeFilters)
        {
            switch (filter.FilterType)
            {
                case "Extension":
                    var extension = Path.GetExtension(filePath).ToLowerInvariant();
                    if (extension == filter.FilterValue.ToLowerInvariant())
                    {
                        var reason = filter.IsDefault
                            ? $"Extension {extension} exclue par filtre par défaut"
                            : $"Extension {extension} exclue par filtre personnalisé";
                        return (true, reason);
                    }
                    break;

                case "SizeRange":
                    // Format attendu: "0-1KB", "0-100B", "10MB-50MB"
                    if (IsInSizeRange(fileSize, filter.FilterValue))
                    {
                        return (true, $"Taille de fichier exclue par filtre : {filter.FilterValue}");
                    }
                    break;

                case "Pattern":
                    // Pattern de nom de fichier (ex: "*.tmp", "*backup*")
                    if (MatchesPattern(Path.GetFileName(filePath), filter.FilterValue))
                    {
                        return (true, $"Nom de fichier exclu par pattern : {filter.FilterValue}");
                    }
                    break;
            }
        }

        return (false, null);
    }

    public async Task<IEnumerable<string>> GetDefaultExcludedExtensionsAsync()
    {
        var defaultFilters = await _filterRepository.GetDefaultFiltersAsync();
        return defaultFilters
            .Where(f => f.FilterType == "Extension" && f.IsActive)
            .Select(f => f.FilterValue)
            .ToList();
    }

    public async Task<IEnumerable<ExclusionFilter>> GetActiveFiltersAsync()
    {
        return await _filterRepository.GetActiveFiltersAsync();
    }

    public async Task AddCustomFilterAsync(string filterType, string filterValue, string? description = null)
    {
        // Vérifier si le filtre existe déjà
        if (await _filterRepository.ExistsAsync(filterType, filterValue))
        {
            throw new InvalidOperationException($"Le filtre {filterType}:{filterValue} existe déjà");
        }

        var filter = new ExclusionFilter
        {
            FilterType = filterType,
            FilterValue = filterValue,
            IsDefault = false,
            IsActive = true,
            Description = description ?? $"Filtre personnalisé : {filterType} = {filterValue}"
        };

        await _filterRepository.AddAsync(filter);
    }

    public async Task SetFilterActiveAsync(int filterId, bool isActive)
    {
        var filter = await _filterRepository.GetByIdAsync(filterId);
        if (filter == null)
        {
            throw new ArgumentException($"Filtre {filterId} introuvable");
        }

        filter.IsActive = isActive;
        await _filterRepository.UpdateAsync(filter);
    }

    public async Task DeleteCustomFilterAsync(int filterId)
    {
        var filter = await _filterRepository.GetByIdAsync(filterId);
        if (filter == null)
        {
            throw new ArgumentException($"Filtre {filterId} introuvable");
        }

        if (filter.IsDefault)
        {
            throw new InvalidOperationException("Impossible de supprimer un filtre par défaut");
        }

        await _filterRepository.DeleteAsync(filterId);
    }

    private bool IsInSizeRange(long fileSize, string range)
    {
        // Parse range format: "0-1KB", "10MB-50MB", "0-100B"
        try
        {
            var parts = range.Split('-');
            if (parts.Length != 2) return false;

            long minSize = ParseSize(parts[0].Trim());
            long maxSize = ParseSize(parts[1].Trim());

            return fileSize >= minSize && fileSize <= maxSize;
        }
        catch
        {
            return false;
        }
    }

    private long ParseSize(string sizeStr)
    {
        sizeStr = sizeStr.ToUpperInvariant();

        long multiplier = 1;
        if (sizeStr.EndsWith("KB"))
        {
            multiplier = 1024;
            sizeStr = sizeStr[..^2];
        }
        else if (sizeStr.EndsWith("MB"))
        {
            multiplier = 1024 * 1024;
            sizeStr = sizeStr[..^2];
        }
        else if (sizeStr.EndsWith("GB"))
        {
            multiplier = 1024 * 1024 * 1024;
            sizeStr = sizeStr[..^2];
        }
        else if (sizeStr.EndsWith("B"))
        {
            sizeStr = sizeStr[..^1];
        }

        return long.Parse(sizeStr) * multiplier;
    }

    private bool MatchesPattern(string fileName, string pattern)
    {
        // Conversion simple de pattern wildcard en regex
        // * = n'importe quels caractères
        // ? = un caractère
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(
            fileName,
            regexPattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}

