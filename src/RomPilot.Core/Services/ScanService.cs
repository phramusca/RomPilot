using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Archives;
using RomPilot.Core.Checksums;
using RomPilot.Core.Database;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;
using RomPilot.Core.Services;

namespace RomPilot.Core.Services;

/// <summary>
/// Service that orchestrates the scan workflow: scanning directories, calculating checksums, and identifying games.
/// </summary>
public class ScanService : IScanService
{
    private readonly RomPilotDbContext _context;
    private readonly IArchiveScanner _archiveScanner;
    private readonly IChecksumCalculator _checksumCalculator;
    private readonly IConsoleDetectionService _consoleDetectionService;
    private readonly IGameIdentificationService _gameIdentificationService;
    private readonly IRomFileRepository _romFileRepository;
    private readonly IChecksumRepository _checksumRepository;

    public ScanService(
        RomPilotDbContext context,
        IArchiveScanner archiveScanner,
        IChecksumCalculator checksumCalculator,
        IConsoleDetectionService consoleDetectionService,
        IGameIdentificationService gameIdentificationService,
        IRomFileRepository romFileRepository,
        IChecksumRepository checksumRepository)
    {
        _context = context;
        _archiveScanner = archiveScanner;
        _checksumCalculator = checksumCalculator;
        _consoleDetectionService = consoleDetectionService;
        _gameIdentificationService = gameIdentificationService;
        _romFileRepository = romFileRepository;
        _checksumRepository = checksumRepository;
    }

    public async Task<IEnumerable<RomFile>> ScanDirectoriesAsync(
        IEnumerable<string> directoryPaths,
        IScanProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<RomFile>();
        var allArchiveFiles = new List<ArchiveFileInfo>();

        // Step 1: Scan all directories for ROM files
        foreach (var directoryPath in directoryPaths)
        {
            if (!Directory.Exists(directoryPath))
            {
                progressReporter?.ReportProgress(0, 0, $"Directory not found: {directoryPath}");
                continue;
            }

            progressReporter?.ReportProgress(0, 0, $"Scanning directory: {directoryPath}");
            var archiveFiles = await _archiveScanner.ScanDirectoryAsync(directoryPath, 5, cancellationToken);
            allArchiveFiles.AddRange(archiveFiles);
        }

        var totalFiles = allArchiveFiles.Count;
        progressReporter?.ReportProgress(0, totalFiles, $"Found {totalFiles} ROM files");

        // Step 2: Process each file
        for (int i = 0; i < allArchiveFiles.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var archiveFile = allArchiveFiles[i];
            
            progressReporter?.ReportProgress(i + 1, totalFiles, $"Processing: {Path.GetFileName(archiveFile.FilePath)}");
            progressReporter?.ReportFileProcessing(archiveFile.FilePath);

            try
            {
                // Detect console
                var consoleShortName = await _consoleDetectionService.DetectConsoleAsync(
                    archiveFile.FilePath, 
                    archiveFile.FileSize, 
                    null);
                if (string.IsNullOrEmpty(consoleShortName))
                {
                    progressReporter?.ReportProgress(i + 1, totalFiles, $"Could not detect console for: {archiveFile.FilePath}");
                    continue;
                }

                // Get console from repository
                var console = await _context.Consoles.FirstOrDefaultAsync(c => c.ShortName == consoleShortName);
                if (console == null)
                {
                    progressReporter?.ReportProgress(i + 1, totalFiles, $"Console not found in database: {consoleShortName}");
                    continue;
                }

                progressReporter?.ReportRomFound(archiveFile.FilePath, console.Name);

                // Get or create RomFile
                var romFile = await GetOrCreateRomFileAsync(archiveFile, console.Id);

                // Calculate checksums
                var checksums = await CalculateChecksumsAsync(archiveFile, cancellationToken);
                await SaveChecksumsAsync(romFile, checksums);

                // Identify game
                var gameEntry = await _gameIdentificationService.IdentifyGameAsync(romFile, checksums, cancellationToken);
                if (gameEntry != null)
                {
                    // Link game to ROM (this would be done via GameRomVersion in a full implementation)
                    progressReporter?.ReportProgress(i + 1, totalFiles, $"Identified: {gameEntry.GameName}");
                }

                results.Add(romFile);
            }
            catch (Exception ex)
            {
                progressReporter?.ReportProgress(i + 1, totalFiles, $"Error processing {archiveFile.FilePath}: {ex.Message}");
            }
        }

        return results;
    }

    private async Task<RomFile> GetOrCreateRomFileAsync(ArchiveFileInfo archiveFile, int consoleId)
    {
        // Check if ROM file already exists
        var existing = await _context.RomFiles
            .FirstOrDefaultAsync(rf => rf.FilePath == archiveFile.FilePath && rf.ArchivePath == archiveFile.ArchivePath);

        if (existing != null)
        {
            existing.LastScannedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        // Create new RomFile
        var romFile = new RomFile
        {
            FilePath = archiveFile.FilePath,
            FileName = Path.GetFileName(archiveFile.FilePath),
            FileSize = archiveFile.FileSize,
            ArchivePath = archiveFile.ArchivePath,
            ArchiveDepth = archiveFile.ArchiveDepth,
            ConsoleId = consoleId,
            DetectedAt = DateTime.UtcNow,
            LastScannedAt = DateTime.UtcNow
        };

        return await _romFileRepository.AddAsync(romFile);
    }

    private async Task<Dictionary<string, string>> CalculateChecksumsAsync(
        ArchiveFileInfo archiveFile,
        CancellationToken cancellationToken)
    {
        Stream? stream = null;
        try
        {
            if (!string.IsNullOrEmpty(archiveFile.ArchivePath))
            {
                // Extract from archive
                stream = await _archiveScanner.ExtractFileAsync(archiveFile.ArchivePath, archiveFile.FilePath);
            }
            else
            {
                // Direct file
                stream = File.OpenRead(archiveFile.FilePath);
            }

            return await _checksumCalculator.CalculateAllAsync(stream);
        }
        finally
        {
            stream?.Dispose();
        }
    }

    private async Task SaveChecksumsAsync(RomFile romFile, Dictionary<string, string> checksums)
    {
        foreach (var (hashType, hashValue) in checksums)
        {
            // Check if checksum already exists
            var existing = await _checksumRepository.GetByHashAsync(hashType, hashValue);
            if (existing == null)
            {
                var checksum = new Checksum
                {
                    RomFileId = romFile.Id,
                    HashType = hashType,
                    HashValue = hashValue,
                    CalculatedAt = DateTime.UtcNow
                };
                await _checksumRepository.AddAsync(checksum);
            }
        }
    }
}

