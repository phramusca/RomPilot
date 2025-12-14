using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for ExclusionFilter entities.
/// </summary>
public interface IExclusionFilterRepository
{
    Task<ExclusionFilter?> GetByIdAsync(int id);
    Task<IEnumerable<ExclusionFilter>> GetActiveFiltersAsync();
    Task<IEnumerable<ExclusionFilter>> GetDefaultFiltersAsync();
    Task<IEnumerable<ExclusionFilter>> GetCustomFiltersAsync();
    Task<IEnumerable<ExclusionFilter>> GetByTypeAsync(string filterType);
    Task<IEnumerable<ExclusionFilter>> GetAllAsync();
    Task<ExclusionFilter> AddAsync(ExclusionFilter filter);
    Task UpdateAsync(ExclusionFilter filter);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string filterType, string filterValue);
    Task SetFilterActiveAsync(int filterId, bool isActive);
}

