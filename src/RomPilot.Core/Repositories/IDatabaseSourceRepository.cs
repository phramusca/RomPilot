using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for DatabaseSource entities.
/// </summary>
public interface IDatabaseSourceRepository
{
    Task<DatabaseSource?> GetByIdAsync(int id);
    Task<DatabaseSource?> GetByNameAsync(string name);
    Task<IEnumerable<DatabaseSource>> GetAllAsync();
    Task<IEnumerable<DatabaseSource>> GetActiveAsync();
    Task<DatabaseSource> AddAsync(DatabaseSource databaseSource);
    Task UpdateAsync(DatabaseSource databaseSource);
    Task DeleteAsync(int id);
}

