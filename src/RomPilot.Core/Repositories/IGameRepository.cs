using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for Game entities.
/// </summary>
public interface IGameRepository
{
    Task<Game?> GetByIdAsync(int id);
    Task<IEnumerable<Game>> GetByConsoleIdAsync(int consoleId);
    Task<Game?> GetByNameAndConsoleAsync(string name, int consoleId);
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game> AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(int id);
}

