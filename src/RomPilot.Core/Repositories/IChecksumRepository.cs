using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for Checksum entities.
/// </summary>
public interface IChecksumRepository
{
    Task<IEnumerable<Checksum>> GetByScannedFileIdAsync(int scannedFileId);
    Task<Checksum?> GetByHashAsync(string hashType, string hashValue);
    Task<Checksum> AddAsync(Checksum checksum);
    Task UpdateAsync(Checksum checksum);
    Task DeleteAsync(int id);
}

