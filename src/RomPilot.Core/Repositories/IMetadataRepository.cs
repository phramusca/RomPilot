using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for Metadata entities.
/// </summary>
public interface IMetadataRepository
{
    Task<Metadata?> GetByIdAsync(int id);
    Task<IEnumerable<Metadata>> GetByGameIdAsync(int gameId);
    Task<Metadata?> GetByGameIdAndSourceAsync(int gameId, string source);
    Task<Metadata> AddAsync(Metadata metadata);
    Task UpdateAsync(Metadata metadata);
    Task DeleteAsync(int id);
}

