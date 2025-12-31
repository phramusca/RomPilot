using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Archives;
using RomPilot.Core.Checksums;
using RomPilot.Core.Database;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;
using RomPilot.Core.Services;

namespace RomPilot.Core.Services;

/// <summary>
/// Service that orchestrates the scan workflow: scanning directories, filtering files, calculating checksums, and identifying games.
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
    private readonly IFilterService _filterService;

    public ScanService(
        RomPilotDbContext context,
        IArchiveScanner archiveScanner,
        IChecksumCalculator checksumCalculator,
        IConsoleDetectionService consoleDetectionService,
        IGameIdentificationService gameIdentificationService,
        IScannedFileRepository scannedFileRepository,
        IChecksumRepository checksumRepository,
        IFilterService filterService)
    {
        _context = context;
        _archiveScanner = archiveScanner;
        _checksumCalculator = checksumCalculator;
        _consoleDetectionService = consoleDetectionService;
        _gameIdentificationService = gameIdentificationService;
        _scannedFileRepository = scannedFileRepository;
        _checksumRepository = checksumRepository;
        _filterService = filterService;
    }

    public async Task<IEnumerable<ScannedFile>> ScanDirectoriesAsync(
        IEnumerable<string> directoryPaths,
        Models.ScanType scanType = Models.ScanType.Quick,
        IScanProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ScannedFile>();
        var allArchiveFiles = new List<ArchiveFileInfo>();

        System.Console.WriteLine($"[ScanService] Starting scan with type: {scanType}");
        progressReporter?.ReportProgress(0, 0, $"Scan type: {scanType}");

        // Step 0: Check if reference databases exist
        var hasReferenceDatabases = await _context.ReferenceDatabases
            .AnyAsync(rd => rd.DownloadStatus == "Downloaded", cancellationToken);

        var hasGameEntries = await _context.GameEntries.AnyAsync(cancellationToken);

        if (!hasReferenceDatabases || !hasGameEntries)
        {
            var warningMsg = "⚠️ AVERTISSEMENT : Aucune base de données de référence téléchargée. " +
                           "Les fichiers seront scannés mais ne pourront pas être identifiés comme ROMs. " +
                           "Veuillez d'abord télécharger des bases de données (NoIntro, Redump, GoodSet) " +
                           "via le gestionnaire de bases de données.";
            System.Console.WriteLine($"[ScanService] {warningMsg}");
            progressReporter?.ReportProgress(0, 0, warningMsg);

            // Continue the scan anyway - files will be stored as "Unidentified"
            // This allows users to see what files they have before downloading databases
        }
        else
        {
            var dbCount = await _context.ReferenceDatabases
                .CountAsync(rd => rd.DownloadStatus == "Downloaded", cancellationToken);
            var entryCount = await _context.GameEntries.CountAsync(cancellationToken);
            System.Console.WriteLine($"[ScanService] Found {dbCount} downloaded databases with {entryCount} game entries");
            progressReporter?.ReportProgress(0, 0, $"{dbCount} databases loaded with {entryCount} entries");
        }

        // Step 1: Scan all directories for files
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
        var msg3 = $"Found {totalFiles} files to scan";
        System.Console.WriteLine($"[ScanService] {msg3}");
        progressReporter?.ReportProgress(0, totalFiles, msg3);

        // Step 1.5: Filter excluded files
        System.Console.WriteLine($"[ScanService] Filtering files with FilterService...");
        var filesToProcess = new List<ArchiveFileInfo>();
        var excludedFiles = new List<(ArchiveFileInfo file, string reason)>();

        foreach (var file in allArchiveFiles)
        {
            var (shouldExclude, reason) = await _filterService.ShouldExcludeFileAsync(file.FilePath, file.FileSize);
            if (shouldExclude)
            {
                System.Console.WriteLine($"[ScanService] File excluded: {Path.GetFileName(file.FilePath)} - {reason}");
                excludedFiles.Add((file, reason!));
                progressReporter?.ReportFileExcluded(file.FilePath, reason!);

                // Save excluded file to database with status "Excluded"
                try
                {
                    var excludedScannedFile = new ScannedFile
                    {
                        FilePath = file.FilePath,
                        FileName = Path.GetFileName(file.FilePath),
                        FileSize = file.FileSize,
                        LastModifiedTimestamp = file.LastModifiedTimestamp,
                        ArchivePath = file.ArchivePath,
                        FilePathInArchive = file.FilePathInArchive,
                        ArchiveDepth = file.ArchiveDepth,
                        IdentificationStatus = "Excluded",
                        ExclusionReason = reason,
                        LastScannedAt = DateTime.UtcNow,
                        ScanType = scanType.ToString()
                    };

                    // Check if file already exists
                    var existing = await _context.ScannedFiles
                        .FirstOrDefaultAsync(sf => sf.FilePath == file.FilePath && sf.ArchivePath == file.ArchivePath);
                    if (existing != null)
                    {
                        // Update existing
                        existing.IdentificationStatus = "Excluded";
                        existing.ExclusionReason = reason;
                        existing.LastScannedAt = DateTime.UtcNow;
                        existing.ScanType = scanType.ToString();
                        existing.LastModifiedTimestamp = file.LastModifiedTimestamp;
                        existing.FileSize = file.FileSize;
                        excludedScannedFile = existing;
                    }
                    else
                    {
                        await _context.ScannedFiles.AddAsync(excludedScannedFile);
                    }
                    await _context.SaveChangesAsync();

                    // Add to results so excluded files are returned
                    results.Add(excludedScannedFile);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"[ScanService] Failed to save excluded file: {ex.Message}");
                }
            }
            else
            {
                filesToProcess.Add(file);
            }
        }

        System.Console.WriteLine($"[ScanService] Filtering complete: {filesToProcess.Count} files to process, {excludedFiles.Count} files excluded");
        progressReporter?.ReportProgress(0, filesToProcess.Count, $"{filesToProcess.Count} files to process, {excludedFiles.Count} excluded");

        // Step 2: Process each non-excluded file
        for (int i = 0; i < filesToProcess.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var archiveFile = filesToProcess[i];

            progressReporter?.ReportProgress(i + 1, filesToProcess.Count, $"Processing: {Path.GetFileName(archiveFile.FilePath)}");
            progressReporter?.ReportFileProcessing(archiveFile.FilePath);

            try
            {
                // NO automatic console detection by extension!
                // Console will ONLY be identified via checksum matching in reference databases

                // Get or create ScannedFile (without console association)
                System.Console.WriteLine($"[ScanService] Getting or creating ScannedFile for: {archiveFile.FilePath}");
                var scannedFile = await GetOrCreateScannedFileAsync(archiveFile, null, scanType);
                System.Console.WriteLine($"[ScanService] ScannedFile created/retrieved: ID={scannedFile.Id}, FilePath={scannedFile.FilePath}");

                // Get or calculate checksums
                Dictionary<string, string> checksums;
                var needsRecalculation = await ShouldRecalculateChecksumsAsync(scannedFile, archiveFile, scanType);
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

                // Identify game via checksum matching in reference databases
                System.Console.WriteLine($"[ScanService] Identifying game via checksums for: {scannedFile.FilePath}");
                var gameEntry = await _gameIdentificationService.IdentifyGameAsync(scannedFile, checksums, cancellationToken);

                if (gameEntry != null)
                {
                    // Game identified via checksum! Set console from the game entry
                    System.Console.WriteLine($"[ScanService] Game identified: {gameEntry.GameName} (Console: {gameEntry.Console?.Name})");
                    scannedFile.IdentificationStatus = "Identified";
                    scannedFile.ConsoleId = gameEntry.ConsoleId;
                    scannedFile.FailureReason = null;
                    progressReporter?.ReportFileSuccess(archiveFile.FilePath, "game_identified", gameEntry.GameName);
                    progressReporter?.ReportProgress(i + 1, totalFiles, $"Identified: {gameEntry.GameName}");
                }
                else
                {
                    // No match in reference databases - leave as unidentified
                    System.Console.WriteLine($"[ScanService] No game identified for: {scannedFile.FilePath}");
                    scannedFile.IdentificationStatus = "Unidentified";
                    scannedFile.ConsoleId = null; // NO console association without identification!
                    scannedFile.FailureReason = null;
                    progressReporter?.ReportFileSuccess(archiveFile.FilePath, "unidentified", null);
                    progressReporter?.ReportProgress(i + 1, totalFiles, $"Unidentified");
                }

                await _context.SaveChangesAsync(); // Save status update

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

        // Load Console navigation property for all results
        foreach (var file in results.Where(f => f.ConsoleId.HasValue && f.ConsoleId.Value > 0))
        {
            if (file.Console == null && file.ConsoleId.HasValue)
            {
                file.Console = await _context.Consoles.FindAsync(file.ConsoleId.Value);
            }
        }

        // Clean up temporary files that were created during nested archive scanning
        // These are files in /tmp that were used as ImmediateArchivePath
        CleanupTemporaryArchiveFiles(allArchiveFiles);

        return results;
    }

    private async Task<ScannedFile> GetOrCreateScannedFileAsync(ArchiveFileInfo archiveFile, int? consoleId, Models.ScanType scanType)
    {
        try
        {
            // Check if file already exists
            System.Console.WriteLine($"[ScanService] Checking if file exists: FilePath={archiveFile.FilePath}, ArchivePath={archiveFile.ArchivePath}");

            // First check if it's already tracked
            var tracked = _context.ScannedFiles.Local.FirstOrDefault(rf => rf.FilePath == archiveFile.FilePath && rf.ArchivePath == archiveFile.ArchivePath);
            if (tracked != null)
            {
                System.Console.WriteLine($"[ScanService] File already tracked (ID={tracked.Id}), updating metadata");
                tracked.LastScannedAt = DateTime.UtcNow;
                tracked.ScanType = scanType.ToString();
                tracked.LastModifiedTimestamp = archiveFile.LastModifiedTimestamp;
                tracked.FileSize = archiveFile.FileSize;
                return tracked;
            }

            // If not tracked, query from database
            var existing = await _context.ScannedFiles
                .FirstOrDefaultAsync(rf => rf.FilePath == archiveFile.FilePath && rf.ArchivePath == archiveFile.ArchivePath);

            if (existing != null)
            {
                System.Console.WriteLine($"[ScanService] File already exists (ID={existing.Id}), updating metadata");
                existing.LastScannedAt = DateTime.UtcNow;
                existing.ScanType = scanType.ToString();
                existing.LastModifiedTimestamp = archiveFile.LastModifiedTimestamp;
                existing.FileSize = archiveFile.FileSize;
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
                FilePathInArchive = archiveFile.FilePathInArchive,
                ArchiveDepth = archiveFile.ArchiveDepth,
                ConsoleId = consoleId,
                IdentificationStatus = "Unidentified",
                DetectedAt = DateTime.UtcNow,
                LastScannedAt = DateTime.UtcNow,
                ScanType = scanType.ToString()
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
                // Extract from archive - use source archive path and FilePathInArchive for extraction
                // FilePathInArchive contains the relative path within the archive (e.g., "inner.zip/nested_game.nes")
                var filePathInArchive = !string.IsNullOrEmpty(archiveFile.FilePathInArchive)
                    ? archiveFile.FilePathInArchive
                    : Path.GetFileName(archiveFile.FilePath); // Fallback if FilePathInArchive is empty
                stream = await _archiveScanner.ExtractFileAsync(archiveFile.ArchivePath, filePathInArchive);
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

    private async Task<bool> ShouldRecalculateChecksumsAsync(ScannedFile scannedFile, ArchiveFileInfo archiveFile, Models.ScanType scanType)
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

        // If scan type is Full, always recalculate
        if (scanType == Models.ScanType.Full)
        {
            System.Console.WriteLine($"[ScanService] Full scan mode: recalculating checksums for {scannedFile.FilePath}");
            return true;
        }

        // If scan type is Quick, compare timestamp and file size to detect changes
        // If both match, file is unchanged -> reuse existing checksums
        bool timestampMatches = scannedFile.LastModifiedTimestamp == archiveFile.LastModifiedTimestamp;
        bool sizeMatches = scannedFile.FileSize == archiveFile.FileSize;

        if (timestampMatches && sizeMatches)
        {
            System.Console.WriteLine($"[ScanService] Quick scan: file unchanged (timestamp and size match), reusing checksums for {scannedFile.FilePath}");
            return false; // Reuse existing checksums
        }

        System.Console.WriteLine($"[ScanService] Quick scan: file changed (timestamp={timestampMatches}, size={sizeMatches}), recalculating checksums for {scannedFile.FilePath}");
        return true; // Recalculate checksums
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
                // New checksum - always store it, even if another file has the same checksum value
                // This allows duplicate detection: multiple files can have the same checksum
                System.Console.WriteLine($"[ScanService] Adding new checksum for {scannedFile.FilePath} ({hashType})");
                
                // Check if this checksum value exists for another file (for duplicate detection info)
                var existingByHash = await _checksumRepository.GetByHashAsync(hashType, hashValue);
                if (existingByHash != null)
                {
                    System.Console.WriteLine($"[ScanService] Checksum {hashType} value {hashValue} already exists for file ID {existingByHash.ScannedFileId} - potential duplicate detected");
                }
                
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

