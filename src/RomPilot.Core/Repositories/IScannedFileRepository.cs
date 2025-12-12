using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository interface for ScannedFile entities (renommé de IRomFileRepository).
/// </summary>
public interface IScannedFileRepository
{
    Task<ScannedFile?> GetByIdAsync(int id);
    Task<IEnumerable<ScannedFile>> GetByConsoleIdAsync(int consoleId);
    Task<ScannedFile?> GetByChecksumAsync(string hashType, string hashValue);
    Task<ScannedFile?> GetByFilePathAsync(string filePath);
    Task<IEnumerable<ScannedFile>> GetByIdentificationStatusAsync(string status);
    Task<IEnumerable<ScannedFile>> GetUnidentifiedFilesAsync();
    Task<IEnumerable<ScannedFile>> GetIdentifiedRomsAsync();
    Task<IEnumerable<ScannedFile>> GetExcludedFilesAsync();
    Task<IEnumerable<ScannedFile>> GetFailedFilesAsync();
    Task<IEnumerable<ScannedFile>> GetAllAsync();
    Task<ScannedFile> AddAsync(ScannedFile scannedFile);
    Task UpdateAsync(ScannedFile scannedFile);
    Task DeleteAsync(int id);
    Task<bool> ExistsByPathAsync(string filePath, long lastModifiedTimestamp);
}

