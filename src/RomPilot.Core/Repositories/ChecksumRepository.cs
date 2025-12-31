using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for Checksum entities.
/// </summary>
public class ChecksumRepository : IChecksumRepository
{
    private readonly RomPilotDbContext _context;

    public ChecksumRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Checksum>> GetByScannedFileIdAsync(int scannedFileId)
    {
        return await _context.Checksums
            .Where(c => c.ScannedFileId == scannedFileId)
            .ToListAsync();
    }

    public async Task<Checksum?> GetByHashAsync(string hashType, string hashValue)
    {
        return await _context.Checksums
            .Include(c => c.ScannedFile)
            .FirstOrDefaultAsync(c => c.HashType == hashType && c.HashValue == hashValue);
    }

    /// <summary>
    /// Gets all checksums with the same hash value (for duplicate detection).
    /// Returns all files that have the same checksum, allowing duplicate detection.
    /// </summary>
    public async Task<IEnumerable<Checksum>> GetAllByHashAsync(string hashType, string hashValue)
    {
        return await _context.Checksums
            .Include(c => c.ScannedFile)
            .Where(c => c.HashType == hashType && c.HashValue == hashValue)
            .ToListAsync();
    }

    public async Task<Checksum> AddAsync(Checksum checksum)
    {
        _context.Checksums.Add(checksum);
        await _context.SaveChangesAsync();
        return checksum;
    }

    public async Task UpdateAsync(Checksum checksum)
    {
        _context.Checksums.Update(checksum);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var checksum = await _context.Checksums.FindAsync(id);
        if (checksum != null)
        {
            _context.Checksums.Remove(checksum);
            await _context.SaveChangesAsync();
        }
    }
}

