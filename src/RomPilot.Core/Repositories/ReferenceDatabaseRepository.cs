using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for ReferenceDatabase entities.
/// </summary>
public class ReferenceDatabaseRepository : IReferenceDatabaseRepository
{
    private readonly RomPilotDbContext _context;

    public ReferenceDatabaseRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<ReferenceDatabase?> GetByIdAsync(int id)
    {
        return await _context.ReferenceDatabases.FindAsync(id);
    }

    public async Task<ReferenceDatabase?> GetByProviderConsoleVersionAsync(string provider, string console, string version)
    {
        return await _context.ReferenceDatabases
            .FirstOrDefaultAsync(d => d.Provider == provider && d.Console == console && d.Version == version);
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetByProviderAsync(string provider)
    {
        return await _context.ReferenceDatabases
            .Where(d => d.Provider == provider)
            .OrderByDescending(d => d.ReleaseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetByConsoleAsync(string console)
    {
        return await _context.ReferenceDatabases
            .Where(d => d.Console == console)
            .OrderByDescending(d => d.ReleaseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetDownloadedAsync()
    {
        return await _context.ReferenceDatabases
            .Where(d => d.DownloadStatus == "Downloaded")
            .ToListAsync();
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetByStatusAsync(string status)
    {
        return await _context.ReferenceDatabases
            .Where(d => d.DownloadStatus == status)
            .ToListAsync();
    }

    public async Task<ReferenceDatabase?> GetDefaultForConsoleAsync(string console)
    {
        return await _context.ReferenceDatabases
            .Where(d => d.Console == console && d.IsDefault)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetAllAsync()
    {
        return await _context.ReferenceDatabases
            .OrderBy(d => d.Provider)
            .ThenBy(d => d.Console)
            .ThenByDescending(d => d.ReleaseDate)
            .ToListAsync();
    }

    public async Task<ReferenceDatabase> AddAsync(ReferenceDatabase database)
    {
        _context.ReferenceDatabases.Add(database);
        await _context.SaveChangesAsync();
        return database;
    }

    public async Task UpdateAsync(ReferenceDatabase database)
    {
        database.UpdatedAt = DateTime.UtcNow;
        _context.ReferenceDatabases.Update(database);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var database = await _context.ReferenceDatabases.FindAsync(id);
        if (database != null)
        {
            _context.ReferenceDatabases.Remove(database);
            await _context.SaveChangesAsync();
        }
    }
}

