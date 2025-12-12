using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for ExportConfiguration entities.
/// </summary>
public interface IExportConfigurationRepository
{
    Task<ExportConfiguration?> GetByIdAsync(int id);
    Task<ExportConfiguration?> GetByNameAsync(string name);
    Task<IEnumerable<ExportConfiguration>> GetByPlatformAsync(string platform);
    Task<IEnumerable<ExportConfiguration>> GetByExportTypeAsync(string exportType);
    Task<IEnumerable<ExportConfiguration>> GetActiveConfigurationsAsync();
    Task<IEnumerable<ExportConfiguration>> GetAllAsync();
    Task<ExportConfiguration> AddAsync(ExportConfiguration configuration);
    Task UpdateAsync(ExportConfiguration configuration);
    Task DeleteAsync(int id);
}

