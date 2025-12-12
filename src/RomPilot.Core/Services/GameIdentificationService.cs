using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Services;

/// <summary>
/// Service for identifying games from ROM files using checksums and database sources.
/// </summary>
public class GameIdentificationService : IGameIdentificationService
{
    private readonly RomPilotDbContext _context;

    public GameIdentificationService(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<GameEntry?> IdentifyGameAsync(
        ScannedFile scannedFile,
        Dictionary<string, string> checksums,
        CancellationToken cancellationToken = default)
    {
        // Try to find matching game entry by checksum in database
        foreach (var (hashType, hashValue) in checksums)
        {
            var gameEntry = await _context.GameEntries
                .Include(ge => ge.ReferenceDatabase)
                .Include(ge => ge.Console)
                .FirstOrDefaultAsync(
                    ge => ge.HashType == hashType && ge.HashValue == hashValue,
                    cancellationToken);
            
            if (gameEntry != null)
            {
                return gameEntry;
            }
        }

        return null;
    }
}

