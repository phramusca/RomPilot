using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for Checksum entities.
/// </summary>
public interface IChecksumRepository
{
    Task<IEnumerable<Checksum>> GetByScannedFileIdAsync(int scannedFileId);
    Task<Checksum?> GetByHashAsync(string hashType, string hashValue);
    /// <summary>
    /// Gets all checksums with the same hash value (for duplicate detection).
    /// Returns all files that have the same checksum, allowing duplicate detection.
    /// </summary>
    Task<IEnumerable<Checksum>> GetAllByHashAsync(string hashType, string hashValue);
    Task<Checksum> AddAsync(Checksum checksum);
    Task UpdateAsync(Checksum checksum);
    Task DeleteAsync(int id);
}

