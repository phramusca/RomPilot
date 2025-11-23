using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for DatabaseSource entities.
/// </summary>
public class DatabaseSourceRepository : IDatabaseSourceRepository
{
    private readonly RomPilotDbContext _context;

    public DatabaseSourceRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<DatabaseSource?> GetByIdAsync(int id)
    {
        return await _context.DatabaseSources.FindAsync(id);
    }

    public async Task<DatabaseSource?> GetByNameAsync(string name)
    {
        return await _context.DatabaseSources
            .FirstOrDefaultAsync(d => d.Name == name);
    }

    public async Task<IEnumerable<DatabaseSource>> GetAllAsync()
    {
        return await _context.DatabaseSources.ToListAsync();
    }

    public async Task<IEnumerable<DatabaseSource>> GetActiveAsync()
    {
        return await _context.DatabaseSources
            .Where(d => d.IsActive)
            .ToListAsync();
    }

    public async Task<DatabaseSource> AddAsync(DatabaseSource databaseSource)
    {
        _context.DatabaseSources.Add(databaseSource);
        await _context.SaveChangesAsync();
        return databaseSource;
    }

    public async Task UpdateAsync(DatabaseSource databaseSource)
    {
        _context.DatabaseSources.Update(databaseSource);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var databaseSource = await _context.DatabaseSources.FindAsync(id);
        if (databaseSource != null)
        {
            _context.DatabaseSources.Remove(databaseSource);
            await _context.SaveChangesAsync();
        }
    }
}

