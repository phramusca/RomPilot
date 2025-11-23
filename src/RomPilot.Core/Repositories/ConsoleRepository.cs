using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for Console entities.
/// </summary>
public class ConsoleRepository : IConsoleRepository
{
    private readonly RomPilotDbContext _context;

    public ConsoleRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<Models.Console?> GetByIdAsync(int id)
    {
        return await _context.Consoles.FindAsync(id);
    }

    public async Task<Models.Console?> GetByShortNameAsync(string shortName)
    {
        return await _context.Consoles
            .FirstOrDefaultAsync(c => c.ShortName == shortName);
    }

    public async Task<IEnumerable<Models.Console>> GetAllAsync()
    {
        return await _context.Consoles.ToListAsync();
    }

    public async Task<Models.Console> AddAsync(Models.Console console)
    {
        _context.Consoles.Add(console);
        await _context.SaveChangesAsync();
        return console;
    }

    public async Task UpdateAsync(Models.Console console)
    {
        _context.Consoles.Update(console);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var console = await _context.Consoles.FindAsync(id);
        if (console != null)
        {
            _context.Consoles.Remove(console);
            await _context.SaveChangesAsync();
        }
    }
}

