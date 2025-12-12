using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for ReferenceDatabase entities.
/// </summary>
public interface IReferenceDatabaseRepository
{
    Task<ReferenceDatabase?> GetByIdAsync(int id);
    Task<ReferenceDatabase?> GetByProviderConsoleVersionAsync(string provider, string console, string version);
    Task<IEnumerable<ReferenceDatabase>> GetByProviderAsync(string provider);
    Task<IEnumerable<ReferenceDatabase>> GetByConsoleAsync(string console);
    Task<IEnumerable<ReferenceDatabase>> GetDownloadedAsync();
    Task<IEnumerable<ReferenceDatabase>> GetByStatusAsync(string status);
    Task<ReferenceDatabase?> GetDefaultForConsoleAsync(string console);
    Task<IEnumerable<ReferenceDatabase>> GetAllAsync();
    Task<ReferenceDatabase> AddAsync(ReferenceDatabase database);
    Task UpdateAsync(ReferenceDatabase database);
    Task DeleteAsync(int id);
}

