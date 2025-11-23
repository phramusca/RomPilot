using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for Console entities.
/// </summary>
public interface IConsoleRepository
{
    Task<Models.Console?> GetByIdAsync(int id);
    Task<Models.Console?> GetByShortNameAsync(string shortName);
    Task<IEnumerable<Models.Console>> GetAllAsync();
    Task<Models.Console> AddAsync(Models.Console console);
    Task UpdateAsync(Models.Console console);
    Task DeleteAsync(int id);
}

