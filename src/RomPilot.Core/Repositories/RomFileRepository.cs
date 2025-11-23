using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for RomFile entities.
/// </summary>
public class RomFileRepository : IRomFileRepository
{
    private readonly RomPilotDbContext _context;

    public RomFileRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<RomFile?> GetByIdAsync(int id)
    {
        return await _context.RomFiles
            .Include(r => r.Console)
            .Include(r => r.Checksums)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<RomFile>> GetByConsoleIdAsync(int consoleId)
    {
        return await _context.RomFiles
            .Where(r => r.ConsoleId == consoleId)
            .Include(r => r.Checksums)
            .ToListAsync();
    }

    public async Task<RomFile?> GetByChecksumAsync(string hashType, string hashValue)
    {
        return await _context.RomFiles
            .Include(r => r.Checksums)
            .Where(r => r.Checksums.Any(c => c.HashType == hashType && c.HashValue == hashValue))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RomFile>> GetAllAsync()
    {
        return await _context.RomFiles
            .Include(r => r.Console)
            .Include(r => r.Checksums)
            .ToListAsync();
    }

    public async Task<RomFile> AddAsync(RomFile romFile)
    {
        _context.RomFiles.Add(romFile);
        await _context.SaveChangesAsync();
        return romFile;
    }

    public async Task UpdateAsync(RomFile romFile)
    {
        _context.RomFiles.Update(romFile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var romFile = await _context.RomFiles.FindAsync(id);
        if (romFile != null)
        {
            _context.RomFiles.Remove(romFile);
            await _context.SaveChangesAsync();
        }
    }
}

