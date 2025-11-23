using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for Metadata entities.
/// </summary>
public class MetadataRepository : IMetadataRepository
{
    private readonly RomPilotDbContext _context;

    public MetadataRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<Metadata?> GetByIdAsync(int id)
    {
        return await _context.Metadata
            .Include(m => m.Game)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Metadata>> GetByGameIdAsync(int gameId)
    {
        return await _context.Metadata
            .Where(m => m.GameId == gameId)
            .ToListAsync();
    }

    public async Task<Metadata?> GetByGameIdAndSourceAsync(int gameId, string source)
    {
        return await _context.Metadata
            .FirstOrDefaultAsync(m => m.GameId == gameId && m.Source == source);
    }

    public async Task<Metadata> AddAsync(Metadata metadata)
    {
        _context.Metadata.Add(metadata);
        await _context.SaveChangesAsync();
        return metadata;
    }

    public async Task UpdateAsync(Metadata metadata)
    {
        _context.Metadata.Update(metadata);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var metadata = await _context.Metadata.FindAsync(id);
        if (metadata != null)
        {
            _context.Metadata.Remove(metadata);
            await _context.SaveChangesAsync();
        }
    }
}

