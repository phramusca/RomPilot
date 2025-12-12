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
    private readonly IScannedFileRepository _scannedFileRepository;
    private readonly IChecksumRepository _checksumRepository;

    public ScanService(
        RomPilotDbContext context,
        IArchiveScanner archiveScanner,
        IChecksumCalculator checksumCalculator,
        IConsoleDetectionService consoleDetectionService,
        IGameIdentificationService gameIdentificationService,
        IScannedFileRepository scannedFileRepository,
        IChecksumRepository checksumRepository)
    {
        _context = context;
        _archiveScanner = archiveScanner;
        _checksumCalculator = checksumCalculator;
        _consoleDetectionService = consoleDetectionService;
        _gameIdentificationService = gameIdentificationService;
        _scannedFileRepository = scannedFileRepository;
        _checksumRepository = checksumRepository;
    }

    public async Task<IEnumerable<ScannedFile>> ScanDirectoriesAsync(
        IEnumerable<string> directoryPaths,
        IScanProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ScannedFile>();
        var allArchiveFiles = new List<ArchiveFileInfo>();

        // Step 1: Scan all directories for ROM files
        foreach (var directoryPath in directoryPaths)
        {
            System.Console.WriteLine($"[ScanService] Starting scan of directory: {directoryPath}");
            
            if (!Directory.Exists(directoryPath))
            {
                var msg = $"Directory not found: {directoryPath}";
                System.Console.WriteLine($"[ScanService] {msg}");
                progressReporter?.ReportProgress(0, 0, msg);
                continue;
            }

            var msg2 = $"Scanning directory: {directoryPath}";
            System.Console.WriteLine($"[ScanService] {msg2}");
            progressReporter?.ReportProgress(0, 0, msg2);
            
            System.Console.WriteLine($"[ScanService] Calling ArchiveScanner.ScanDirectoryAsync...");
            var startTime = DateTime.Now;
            var archiveFiles = await _archiveScanner.ScanDirectoryAsync(directoryPath, 5, progressReporter, cancellationToken);
            var duration = DateTime.Now - startTime;
            var fileList = archiveFiles.ToList();
            System.Console.WriteLine($"[ScanService] ArchiveScanner returned {fileList.Count} files from {directoryPath} in {duration.TotalSeconds:F2} seconds");
            
            allArchiveFiles.AddRange(fileList);
        }

        var totalFiles = allArchiveFiles.Count;
        var msg3 = $"Found {totalFiles} ROM files";
        System.Console.WriteLine($"[ScanService] {msg3}");
        progressReporter?.ReportProgress(0, totalFiles, msg3);

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
                System.Console.WriteLine($"[ScanService] Detecting console for: {archiveFile.FilePath} (size: {archiveFile.FileSize})");
                var consoleShortName = await _consoleDetectionService.DetectConsoleAsync(
                    archiveFile.FilePath, 
                    archiveFile.FileSize, 
                    null);
                System.Console.WriteLine($"[ScanService] Console detection result: {(consoleShortName ?? "null")} for {archiveFile.FilePath}");
                if (string.IsNullOrEmpty(consoleShortName))
                {
                    var failureReason = "Console not detected";
                    System.Console.WriteLine($"[ScanService] {failureReason} for: {archiveFile.FilePath}");
                    progressReporter?.ReportFileFailure(archiveFile.FilePath, failureReason);
                    progressReporter?.ReportProgress(i + 1, totalFiles, failureReason);
                    continue;
                }

                // Get console from repository
                System.Console.WriteLine($"[ScanService] Looking for console '{consoleShortName}' in database...");
                var console = await _context.Consoles.FirstOrDefaultAsync(c => c.ShortName == consoleShortName);
                if (console == null)
                {
                    var failureReason = $"Console not found in database: {consoleShortName}";
                    System.Console.WriteLine($"[ScanService] {failureReason}");
                    progressReporter?.ReportFileFailure(archiveFile.FilePath, failureReason);
                    progressReporter?.ReportProgress(i + 1, totalFiles, failureReason);
                    continue;
                }
                System.Console.WriteLine($"[ScanService] Found console: {console.Name} (ID: {console.Id})");

                progressReporter?.ReportRomFound(archiveFile.FilePath, console.Name);

                // Get or create ScannedFile
                System.Console.WriteLine($"[ScanService] Getting or creating ScannedFile for: {archiveFile.FilePath}");
                var scannedFile = await GetOrCreateScannedFileAsync(archiveFile, console.Id);
                System.Console.WriteLine($"[ScanService] ScannedFile created/retrieved: ID={scannedFile.Id}, FilePath={scannedFile.FilePath}");

                // Get or calculate checksums
                Dictionary<string, string> checksums;
                var needsRecalculation = await ShouldRecalculateChecksumsAsync(scannedFile);
                if (needsRecalculation)
                {
                    // Calculate checksums
                    System.Console.WriteLine($"[ScanService] Calculating checksums for: {archiveFile.FilePath}");
                    checksums = await CalculateChecksumsAsync(archiveFile, cancellationToken);
                    System.Console.WriteLine($"[ScanService] Calculated {checksums.Count} checksums");
                    await SaveChecksumsAsync(scannedFile, checksums);
                    System.Console.WriteLine($"[ScanService] Checksums saved");
                }
                else
                {
                    // Load existing checksums from database
                    System.Console.WriteLine($"[ScanService] Checksums already exist for: {archiveFile.FilePath}, loading from database");
                    var existingChecksums = await _context.Checksums
                        .Where(c => c.ScannedFileId == scannedFile.Id)
                        .ToListAsync();
                    checksums = existingChecksums.ToDictionary(c => c.HashType, c => c.HashValue);
                    System.Console.WriteLine($"[ScanService] Loaded {checksums.Count} checksums from database");
                }

                // Identify game
                System.Console.WriteLine($"[ScanService] Identifying game for ROM: {scannedFile.FilePath}");
                var gameEntry = await _gameIdentificationService.IdentifyGameAsync(scannedFile, checksums, cancellationToken);
                
                // Update ScannedFile with processing status
                scannedFile.IdentificationStatus = gameEntry != null ? "Identified" : "Unidentified";
                scannedFile.FailureReason = null;
                
                if (gameEntry != null)
                {
                    // Link game to ROM (this would be done via GameRomVersion in a full implementation)
                    System.Console.WriteLine($"[ScanService] Game identified: {gameEntry.GameName}");
                    progressReporter?.ReportFileSuccess(archiveFile.FilePath, "game_identified", gameEntry.GameName);
                    progressReporter?.ReportProgress(i + 1, totalFiles, $"Identified: {gameEntry.GameName}");
                }
                else
                {
                    System.Console.WriteLine($"[ScanService] No game identified for ROM: {scannedFile.FilePath}");
                    progressReporter?.ReportFileSuccess(archiveFile.FilePath, "console_identified", console.Name);
                    progressReporter?.ReportProgress(i + 1, totalFiles, $"Console identified: {console.Name}");
                }

                System.Console.WriteLine($"[ScanService] Adding file to results (current count: {results.Count})");
                results.Add(scannedFile);
                System.Console.WriteLine($"[ScanService] File added to results (new count: {results.Count})");
            }
            catch (Exception ex)
            {
                var failureReason = $"Error: {ex.Message}";
                System.Console.WriteLine($"[ScanService] ERROR processing {archiveFile.FilePath}: {ex.Message}");
                System.Console.WriteLine($"[ScanService] Stack trace: {ex.StackTrace}");
                progressReporter?.ReportFileFailure(archiveFile.FilePath, failureReason);
                progressReporter?.ReportProgress(i + 1, totalFiles, failureReason);
                
                // Try to save failure status to database if we have a ScannedFile
                try
                {
                    var existingFile = await _context.ScannedFiles
                        .FirstOrDefaultAsync(rf => rf.FilePath == archiveFile.FilePath && rf.ArchivePath == archiveFile.ArchivePath);
                    if (existingFile != null)
                    {
                        existingFile.IdentificationStatus = "Failed";
                        existingFile.FailureReason = failureReason;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception saveEx)
                {
                    System.Console.WriteLine($"[ScanService] Failed to save failure status: {saveEx.Message}");
                }
            }
        }

        System.Console.WriteLine($"[ScanService] Returning {results.Count} ROM files from ScanDirectoriesAsync");
        
        // Clean up temporary files that were created during nested archive scanning
        // These are files in /tmp that were used as ImmediateArchivePath
        CleanupTemporaryArchiveFiles(allArchiveFiles);
        
        return results;
    }

    private async Task<ScannedFile> GetOrCreateScannedFileAsync(ArchiveFileInfo archiveFile, int consoleId)
    {
        try
        {
            // Check if ROM file already exists
            System.Console.WriteLine($"[ScanService] Checking if ROM file exists: FilePath={archiveFile.FilePath}, ArchivePath={archiveFile.ArchivePath}");
            
            // First check if it's already tracked
            var tracked = _context.ScannedFiles.Local.FirstOrDefault(rf => rf.FilePath == archiveFile.FilePath && rf.ArchivePath == archiveFile.ArchivePath);
            if (tracked != null)
            {
                System.Console.WriteLine($"[ScanService] ROM file already tracked (ID={tracked.Id}), will update LastScannedAt after checksums");
                tracked.LastScannedAt = DateTime.UtcNow;
                return tracked;
            }
            
            // If not tracked, query from database
            var existing = await _context.ScannedFiles
                .FirstOrDefaultAsync(rf => rf.FilePath == archiveFile.FilePath && rf.ArchivePath == archiveFile.ArchivePath);

            if (existing != null)
            {
                System.Console.WriteLine($"[ScanService] ROM file already exists (ID={existing.Id}), will update LastScannedAt after checksums");
                existing.LastScannedAt = DateTime.UtcNow;
                return existing;
            }

            // Create new ScannedFile
            System.Console.WriteLine($"[ScanService] Creating new ScannedFile: FilePath={archiveFile.FilePath}, ArchivePath={archiveFile.ArchivePath}");
            var scannedFile = new ScannedFile
            {
                FilePath = archiveFile.FilePath,
                FileName = Path.GetFileName(archiveFile.FilePath),
                FileSize = archiveFile.FileSize,
                LastModifiedTimestamp = archiveFile.LastModifiedTimestamp,
                ArchivePath = archiveFile.ArchivePath,
                ArchiveDepth = archiveFile.ArchiveDepth,
                ConsoleId = consoleId,
                IdentificationStatus = "Unidentified",
                DetectedAt = DateTime.UtcNow,
                LastScannedAt = DateTime.UtcNow
            };

            var result = await _scannedFileRepository.AddAsync(scannedFile);
            System.Console.WriteLine($"[ScanService] ScannedFile created successfully (ID={result.Id})");
            return result;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ScanService] ERROR in GetOrCreateScannedFileAsync: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"[ScanService] Inner exception: {ex.InnerException.Message}");
                System.Console.WriteLine($"[ScanService] Inner exception type: {ex.InnerException.GetType().Name}");
            }
            System.Console.WriteLine($"[ScanService] Stack trace: {ex.StackTrace}");
            throw;
        }
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
                // Extract from archive - use immediate archive path for extraction (handles nested archives)
                var archivePathToUse = archiveFile.ImmediateArchivePath ?? archiveFile.ArchivePath;
                stream = await _archiveScanner.ExtractFileAsync(archivePathToUse, archiveFile.FilePath);
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

    private void CleanupTemporaryArchiveFiles(List<ArchiveFileInfo> archiveFiles)
    {
        var tempFiles = archiveFiles
            .Where(af => !string.IsNullOrEmpty(af.ImmediateArchivePath) && af.ImmediateArchivePath.StartsWith("/tmp"))
            .Select(af => af.ImmediateArchivePath!)
            .Distinct()
            .ToList();

        foreach (var tempFile in tempFiles)
        {
            try
            {
                if (File.Exists(tempFile))
                {
                    System.Console.WriteLine($"[ScanService] Cleaning up temporary file: {tempFile}");
                    // Retry deletion in case file is still locked
                    for (int retry = 0; retry < 3; retry++)
                    {
                        try
                        {
                            File.Delete(tempFile);
                            break;
                        }
                        catch (IOException)
                        {
                            if (retry < 2)
                            {
                                Thread.Sleep(50);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ScanService] WARNING: Could not delete temp file {tempFile}: {ex.Message}");
            }
        }
    }

    private async Task<bool> ShouldRecalculateChecksumsAsync(ScannedFile scannedFile)
    {
        // Check if checksums exist for this scanned file
        var existingChecksums = await _context.Checksums
            .Where(c => c.ScannedFileId == scannedFile.Id)
            .ToListAsync();
        
        // If no checksums exist, we need to calculate them
        if (existingChecksums.Count == 0)
        {
            System.Console.WriteLine($"[ScanService] No checksums found for {scannedFile.FilePath}, will calculate");
            return true;
        }
        
        // If checksums exist, we'll recalculate to check for changes
        // (This could be optimized further by comparing file size/timestamp, but for now we recalculate)
        System.Console.WriteLine($"[ScanService] Checksums exist for {scannedFile.FilePath}, will recalculate to check for changes");
        return true;
    }

    private async Task SaveChecksumsAsync(ScannedFile scannedFile, Dictionary<string, string> checksums)
    {
        bool hasChanges = false;
        bool checksumChanged = false;
        
        // Get existing checksums for this scanned file
        var existingChecksums = await _context.Checksums
            .Where(c => c.ScannedFileId == scannedFile.Id)
            .ToDictionaryAsync(c => c.HashType, c => c.HashValue);
        
        foreach (var (hashType, hashValue) in checksums)
        {
            // Check if checksum already exists for this ROM file
            if (existingChecksums.TryGetValue(hashType, out var existingHashValue))
            {
                // Checksum exists - compare values
                if (existingHashValue != hashValue)
                {
                    // Checksum has changed - update it
                    System.Console.WriteLine($"[ScanService] Checksum changed for {scannedFile.FilePath} ({hashType}): {existingHashValue} -> {hashValue}");
                    var existing = await _context.Checksums
                        .FirstOrDefaultAsync(c => c.ScannedFileId == scannedFile.Id && c.HashType == hashType);
                    if (existing != null)
                    {
                        existing.HashValue = hashValue;
                        existing.CalculatedAt = DateTime.UtcNow;
                        checksumChanged = true;
                        hasChanges = true;
                    }
                }
                else
                {
                    // Checksum unchanged - skip
                    System.Console.WriteLine($"[ScanService] Checksum unchanged for {scannedFile.FilePath} ({hashType}), skipping");
                }
            }
            else
            {
                // New checksum - check if this hash value exists for another ROM file
                var existingByHash = await _checksumRepository.GetByHashAsync(hashType, hashValue);
                if (existingByHash == null)
                {
                    System.Console.WriteLine($"[ScanService] Adding new checksum for {scannedFile.FilePath} ({hashType})");
                    var checksum = new Checksum
                    {
                        ScannedFileId = scannedFile.Id,
                        HashType = hashType,
                        HashValue = hashValue,
                        CalculatedAt = DateTime.UtcNow
                    };
                    await _checksumRepository.AddAsync(checksum);
                    hasChanges = true;
                }
                else
                {
                    System.Console.WriteLine($"[ScanService] Checksum {hashType} already exists for another ROM file, skipping");
                }
            }
        }
        
        // Only save if there are changes
        if (hasChanges)
        {
            if (checksumChanged)
            {
                System.Console.WriteLine($"[ScanService] Checksum changes detected for {scannedFile.FilePath}");
                // TODO: Store checksum change information (could add a ChecksumHistory table or flag on ScannedFile)
            }
            
            // Save all changes (including LastScannedAt update and checksum changes) in one transaction
            await _context.SaveChangesAsync();
        }
        else
        {
            // No checksum changes, but still update LastScannedAt
            await _context.SaveChangesAsync();
        }
    }
}

