using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for ScannedFile entities (renommé de RomFileRepository).
/// </summary>
public class ScannedFileRepository : IScannedFileRepository
{
    private readonly RomPilotDbContext _context;

    public ScannedFileRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<ScannedFile?> GetByIdAsync(int id)
    {
        return await _context.ScannedFiles
            .Include(r => r.Console)
            .Include(r => r.Checksums)
            .Include(r => r.GameEntry)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<ScannedFile>> GetByConsoleIdAsync(int consoleId)
    {
        return await _context.ScannedFiles
            .Where(r => r.ConsoleId == consoleId)
            .Include(r => r.Checksums)
            .ToListAsync();
    }

    public async Task<ScannedFile?> GetByChecksumAsync(string hashType, string hashValue)
    {
        return await _context.ScannedFiles
            .Include(r => r.Checksums)
            .Where(r => r.Checksums.Any(c => c.HashType == hashType && c.HashValue == hashValue))
            .FirstOrDefaultAsync();
    }

    public async Task<ScannedFile?> GetByFilePathAsync(string filePath)
    {
        return await _context.ScannedFiles
            .FirstOrDefaultAsync(r => r.FilePath == filePath);
    }

    public async Task<IEnumerable<ScannedFile>> GetByIdentificationStatusAsync(string status)
    {
        return await _context.ScannedFiles
            .Where(r => r.IdentificationStatus == status)
            .Include(r => r.Console)
            .Include(r => r.Checksums)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScannedFile>> GetUnidentifiedFilesAsync()
    {
        return await GetByIdentificationStatusAsync("Unidentified");
    }

    public async Task<IEnumerable<ScannedFile>> GetIdentifiedRomsAsync()
    {
        return await GetByIdentificationStatusAsync("Identified");
    }

    public async Task<IEnumerable<ScannedFile>> GetExcludedFilesAsync()
    {
        return await GetByIdentificationStatusAsync("Excluded");
    }

    public async Task<IEnumerable<ScannedFile>> GetFailedFilesAsync()
    {
        return await GetByIdentificationStatusAsync("Failed");
    }

    public async Task<IEnumerable<ScannedFile>> GetAllAsync()
    {
        return await _context.ScannedFiles
            .Include(r => r.Console)
            .Include(r => r.Checksums)
            .ToListAsync();
    }

    public async Task<ScannedFile> AddAsync(ScannedFile scannedFile)
    {
        _context.ScannedFiles.Add(scannedFile);
        await _context.SaveChangesAsync();
        return scannedFile;
    }

    public async Task UpdateAsync(ScannedFile scannedFile)
    {
        _context.ScannedFiles.Update(scannedFile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var scannedFile = await _context.ScannedFiles.FindAsync(id);
        if (scannedFile != null)
        {
            _context.ScannedFiles.Remove(scannedFile);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByPathAsync(string filePath, long lastModifiedTimestamp)
    {
        return await _context.ScannedFiles
            .AnyAsync(r => r.FilePath == filePath && r.LastModifiedTimestamp == lastModifiedTimestamp);
    }
}

