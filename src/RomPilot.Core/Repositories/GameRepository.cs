using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for Game entities.
/// </summary>
public class GameRepository : IGameRepository
{
    private readonly RomPilotDbContext _context;

    public GameRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games
            .Include(g => g.Console)
            .Include(g => g.GameRomVersions)
                .ThenInclude(grv => grv.RomFile)
            .Include(g => g.SelectedRomFile)
            .Include(g => g.SelectedDatabaseSource)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Game>> GetByConsoleIdAsync(int consoleId)
    {
        return await _context.Games
            .Where(g => g.ConsoleId == consoleId)
            .Include(g => g.GameRomVersions)
                .ThenInclude(grv => grv.RomFile)
            .ToListAsync();
    }

    public async Task<Game?> GetByNameAndConsoleAsync(string name, int consoleId)
    {
        return await _context.Games
            .Where(g => g.Name == name && g.ConsoleId == consoleId)
            .Include(g => g.GameRomVersions)
                .ThenInclude(grv => grv.RomFile)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games
            .Include(g => g.Console)
            .Include(g => g.GameRomVersions)
            .ToListAsync();
    }

    public async Task<Game> AddAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game != null)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
    }
}

