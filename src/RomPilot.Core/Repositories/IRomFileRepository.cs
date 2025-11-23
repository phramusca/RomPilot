using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for RomFile entities.
/// </summary>
public interface IRomFileRepository
{
    Task<RomFile?> GetByIdAsync(int id);
    Task<IEnumerable<RomFile>> GetByConsoleIdAsync(int consoleId);
    Task<RomFile?> GetByChecksumAsync(string hashType, string hashValue);
    Task<IEnumerable<RomFile>> GetAllAsync();
    Task<RomFile> AddAsync(RomFile romFile);
    Task UpdateAsync(RomFile romFile);
    Task DeleteAsync(int id);
}

