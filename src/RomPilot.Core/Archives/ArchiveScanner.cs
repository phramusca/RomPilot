using RomPilot.Core.Services;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace RomPilot.Core.Archives;

/// <summary>
/// Implementation of archive scanner for ZIP, 7Z and RAR archives with recursive support.
/// IMPORTANT: Scans ALL files without presuming what is a ROM - filtering is done by FilterService.
/// </summary>
public class ArchiveScanner : IArchiveScanner
{
    // REMOVED: Hard-coded ROM extensions list - no longer presupposing what is a ROM
    // Filtering is now done by FilterService in ScanService

    public async Task<IEnumerable<ArchiveFileInfo>> ScanDirectoryAsync(
        string directoryPath,
        int maxDepth = 5,
        IScanProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ArchiveFileInfo>();
        await ScanDirectoryRecursiveAsync(directoryPath, maxDepth, 0, results, cancellationToken, progressReporter);
        return results;
    }

    private async Task ScanDirectoryRecursiveAsync(
        string path,
        int maxDepth,
        int currentDepth,
        List<ArchiveFileInfo> results,
        CancellationToken cancellationToken,
        IScanProgressReporter? progressReporter = null)
    {
        if (currentDepth > maxDepth || cancellationToken.IsCancellationRequested)
        {
            System.Console.WriteLine($"[ArchiveScanner] Skipping {path} - depth {currentDepth} > maxDepth {maxDepth}");
            return;
        }

        if (!Directory.Exists(path))
        {
            System.Console.WriteLine($"[ArchiveScanner] Directory does not exist: {path}");
            return;
        }

        System.Console.WriteLine($"[ArchiveScanner] Scanning directory: {path} (depth: {currentDepth})");
        progressReporter?.ReportProgress(0, 0, $"Scanning directory: {Path.GetFileName(path)}");

        try
        {
            // Scan direct files
            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            System.Console.WriteLine($"[ArchiveScanner] Found {files.Length} files in {path}");
            progressReporter?.ReportProgress(0, 0, $"Found {files.Length} files in {Path.GetFileName(path)}");

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var extension = Path.GetExtension(file);
                var fileName = Path.GetFileName(file);

                // Check if it's an archive to scan recursively
                if (IsArchiveFile(file) && currentDepth < maxDepth)
                {
                    System.Console.WriteLine($"[ArchiveScanner] Found archive file: {fileName} ({extension}) - will scan contents");
                    progressReporter?.ReportProgress(0, 0, $"Scanning archive: {fileName}");
                    // Scan archive contents - this archive is the source archive, so pass it as originalArchivePath
                    await ScanArchiveAsync(file, currentDepth + 1, maxDepth, results, cancellationToken, file, null, progressReporter);
                }
                else
                {
                    // CHANGED: Scan ALL files without presumption - filtering done later by FilterService
                    var fileInfo = new FileInfo(file);
                    System.Console.WriteLine($"[ArchiveScanner] Found file: {fileName} ({extension}, {fileInfo.Length} bytes)");
                    progressReporter?.ReportProgress(0, 0, $"Found file: {fileName}");
                    results.Add(new ArchiveFileInfo
                    {
                        FilePath = file,
                        ArchivePath = null,
                        ImmediateArchivePath = null,
                        FilePathInArchive = string.Empty, // Empty for files not in archives
                        ArchiveDepth = 0,
                        FileSize = fileInfo.Length,
                        LastModifiedTimestamp = ((DateTimeOffset)fileInfo.LastWriteTimeUtc).ToUnixTimeSeconds()
                    });
                }
            }

            // Scan subdirectories
            var directories = Directory.GetDirectories(path);
            System.Console.WriteLine($"[ArchiveScanner] Found {directories.Length} subdirectories in {path}");
            foreach (var dir in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ScanDirectoryRecursiveAsync(dir, maxDepth, currentDepth, results, cancellationToken, progressReporter);
            }
        }
        catch (Exception ex)
        {
            // Log error but continue scanning
            System.Console.WriteLine($"[ArchiveScanner] ERROR scanning {path}: {ex.Message}");
            System.Console.WriteLine($"[ArchiveScanner] Exception type: {ex.GetType().Name}");
            if (ex.StackTrace != null)
            {
                System.Console.WriteLine($"[ArchiveScanner] Stack trace: {ex.StackTrace}");
            }
        }
    }

    private async Task ScanArchiveAsync(
        string archivePath,
        int currentDepth,
        int maxDepth,
        List<ArchiveFileInfo> results,
        CancellationToken cancellationToken,
        string? originalArchivePath = null,
        string? pathInParentArchive = null,
        IScanProgressReporter? progressReporter = null)
    {
        var archiveName = Path.GetFileName(archivePath);
        System.Console.WriteLine($"[ArchiveScanner] Opening archive: {archiveName} (depth: {currentDepth})");
        progressReporter?.ReportProgress(0, 0, $"Opening archive: {archiveName}");

        try
        {
            IArchive? archive = null;

            if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.WriteLine($"[ArchiveScanner] Opening as ZIP: {archiveName}");
                archive = ZipArchive.Open(archivePath);
            }
            else if (archivePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.WriteLine($"[ArchiveScanner] Opening as 7Z: {archiveName}");
                archive = SevenZipArchive.Open(archivePath);
            }
            else if (archivePath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.WriteLine($"[ArchiveScanner] Opening as RAR: {archiveName}");
                try
                {
                    archive = RarArchive.Open(archivePath);
                    System.Console.WriteLine($"[ArchiveScanner] RAR archive opened successfully: {archiveName}");
                }
                catch (Exception rarEx)
                {
                    System.Console.WriteLine($"[ArchiveScanner] ERROR opening RAR {archiveName}: {rarEx.Message}");
                    System.Console.WriteLine($"[ArchiveScanner] RAR exception type: {rarEx.GetType().Name}");
                    if (rarEx.StackTrace != null)
                    {
                        System.Console.WriteLine($"[ArchiveScanner] RAR stack trace: {rarEx.StackTrace}");
                    }
                    throw; // Re-throw to be caught by outer catch
                }
            }
            else
            {
                System.Console.WriteLine($"[ArchiveScanner] Unknown archive format: {archiveName}");
            }

            if (archive == null)
            {
                System.Console.WriteLine($"[ArchiveScanner] Failed to open archive: {archiveName}");
                return;
            }

            System.Console.WriteLine($"[ArchiveScanner] Successfully opened archive: {archiveName}");

            await Task.Run(async () =>
            {
                var entries = archive.Entries.Where(e => !e.IsDirectory).ToList();
                System.Console.WriteLine($"[ArchiveScanner] Archive {archiveName} contains {entries.Count} files");
                progressReporter?.ReportProgress(0, entries.Count, $"Scanning archive: {archiveName} ({entries.Count} files)");

                int romCount = 0;
                int nestedArchiveCount = 0;
                int skippedCount = 0;
                int processedCount = 0;

                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrEmpty(entry.Key))
                    {
                        skippedCount++;
                        continue;
                    }

                    var extension = Path.GetExtension(entry.Key);
                    var entryName = Path.GetFileName(entry.Key);

                    processedCount++;
                    progressReporter?.ReportProgress(processedCount, entries.Count, $"Scanning {archiveName}: {entryName}");

                    // Check if it's a nested archive to scan recursively
                    if (!string.IsNullOrEmpty(entry.Key) && IsArchiveFile(entry.Key) && currentDepth < maxDepth)
                    {
                        nestedArchiveCount++;
                        System.Console.WriteLine($"[ArchiveScanner] Found nested archive in {archiveName}: {entryName}");
                        progressReporter?.ReportProgress(processedCount, entries.Count, $"Found nested archive: {entryName}");
                        // Nested archive - extract and scan recursively
                        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(entry.Key));
                        try
                        {
                            // Extract to temporary file
                            using (var entryStream = entry.OpenEntryStream())
                            using (var fileStream = File.Create(tempPath))
                            {
                                entryStream.CopyTo(fileStream);
                                fileStream.Flush(); // Ensure all data is written
                            } // Both streams are closed here

                            // Small delay to ensure file system has released the file
                            await Task.Delay(10, cancellationToken).ConfigureAwait(false);

                            // Now scan the extracted archive, but keep reference to original archive path
                            // Use originalArchivePath if available, otherwise use current archivePath (for first level nested archives)
                            var sourceArchivePath = originalArchivePath ?? archivePath;
                            // Build path in parent archive: if we're already in a nested archive, prepend the current path
                            var nestedPathInArchive = pathInParentArchive != null
                                ? $"{pathInParentArchive}/{entryName}"
                                : entryName;
                            await ScanArchiveAsync(tempPath, currentDepth + 1, maxDepth, results, cancellationToken, sourceArchivePath, nestedPathInArchive, progressReporter).ConfigureAwait(false);
                        }
                        catch (Exception nestedEx)
                        {
                            System.Console.WriteLine($"[ArchiveScanner] ERROR processing nested archive {entryName}: {nestedEx.Message}");
                        }
                        // NOTE: We do NOT delete the temporary file here because it may be needed later
                        // for extracting files and calculating checksums. The temporary files will be cleaned up
                        // after the scan is complete and all checksums are calculated.
                        // The file will remain in /tmp and will be cleaned up by the system eventually.
                    }
                    else
                    {
                        // CHANGED: Include ALL files without presumption - filtering done later by FilterService
                        romCount++;
                        if (romCount <= 10) // Log first 10 files
                        {
                            System.Console.WriteLine($"[ArchiveScanner] Found file in {archiveName}: {entryName} ({extension}, {entry.Size} bytes)");
                        }
                        progressReporter?.ReportProgress(processedCount, entries.Count, $"Found file in {archiveName}: {entryName}");
                        // Use original archive path if available (for nested archives), otherwise use current archive path
                        var sourceArchivePath = originalArchivePath ?? archivePath;

                        // Build virtual FilePath: complete path from source archive including nested archives
                        // Example: "/workspace/test-data/medium/nested.zip/inner.zip/nested_game.nes"
                        // This allows easy extraction and comparison for quick scan
                        string virtualFilePath;
                        if (sourceArchivePath != null)
                        {
                            // Build path in archive: if we're in a nested archive, prepend the path in parent archive
                            var filePathInArchive = pathInParentArchive != null
                                ? $"{pathInParentArchive}/{entry.Key}"
                                : entry.Key;
                            // Virtual path: sourceArchivePath + "/" + filePathInArchive
                            virtualFilePath = $"{sourceArchivePath}/{filePathInArchive}";
                        }
                        else
                        {
                            // Should not happen, but fallback
                            virtualFilePath = entry.Key;
                        }

                        // Get last modified time from entry, or fallback to archive file's modification time
                        var lastModified = entry.LastModifiedTime ?? new FileInfo(archivePath).LastWriteTimeUtc;
                        results.Add(new ArchiveFileInfo
                        {
                            FilePath = virtualFilePath, // Virtual path: archive.zip/nested.zip/file.nes
                            ArchivePath = sourceArchivePath, // Original source archive (real file path)
                            ImmediateArchivePath = archivePath, // Immediate archive containing this file (for extraction)
                            FilePathInArchive = pathInParentArchive != null
                                ? $"{pathInParentArchive}/{entry.Key}"
                                : entry.Key, // Path within source archive
                            ArchiveDepth = currentDepth,
                            FileSize = entry.Size,
                            LastModifiedTimestamp = ((DateTimeOffset)lastModified).ToUnixTimeSeconds()
                        });
                    }
                }

                System.Console.WriteLine($"[ArchiveScanner] Archive {archiveName} summary: {romCount} files found, {nestedArchiveCount} nested archives scanned");
            }, cancellationToken);

            archive.Dispose();
            System.Console.WriteLine($"[ArchiveScanner] Finished scanning archive: {archiveName}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ArchiveScanner] ERROR scanning archive {archiveName}: {ex.Message}");
            System.Console.WriteLine($"[ArchiveScanner] Exception type: {ex.GetType().Name}");
            if (ex.StackTrace != null)
            {
                System.Console.WriteLine($"[ArchiveScanner] Stack trace: {ex.StackTrace}");
            }
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"[ArchiveScanner] Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    public async Task<Stream> ExtractFileAsync(string archivePath, string filePathInArchive)
    {
        return await Task.Run(() => ExtractFileRecursive(archivePath, filePathInArchive));
    }

    /// <summary>
    /// Extrait récursivement un fichier depuis une archive, gérant n'importe quel nombre de niveaux d'imbrication.
    /// Exemple: archive1.zip/archive2.zip/archive3.zip/file.nes
    /// </summary>
    private Stream ExtractFileRecursive(string archivePath, string filePathInArchive)
    {
        // Parse filePathInArchive to handle nested archives
        // Example: "inner.zip/nested_game.nes" or "archive1.zip/archive2.zip/file.nes"
        var pathParts = filePathInArchive.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (pathParts.Length == 1)
        {
            // Simple case: file is directly in the archive
            return ExtractFileFromArchive(archivePath, pathParts[0]);
        }
        else
        {
            // Nested archive case: extract nested archive recursively, then extract file from it
            // Example: archivePath = "nested.zip", filePathInArchive = "inner.zip/nested_game.nes"
            // Or: archivePath = "archive1.zip", filePathInArchive = "archive2.zip/archive3.zip/file.nes"
            // 1. Extract first archive part from current archive
            // 2. Recursively extract remaining path from nested archive

            var nestedArchiveName = pathParts[0];
            var remainingPath = string.Join("/", pathParts.Skip(1));

            // Extract nested archive to temp file
            var nestedArchiveStream = ExtractFileFromArchive(archivePath, nestedArchiveName);
            var tempNestedArchive = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(nestedArchiveName));

            try
            {
                using (var fileStream = File.Create(tempNestedArchive))
                {
                    nestedArchiveStream.CopyTo(fileStream);
                }

                // Recursively extract file from nested archive (handles any number of nesting levels)
                var result = ExtractFileRecursive(tempNestedArchive, remainingPath);

                // Cleanup temp file after stream is read (caller's responsibility)
                // Note: We can't delete it here because the stream might not be read yet
                // The temp file will be cleaned up by the system eventually

                return result;
            }
            catch
            {
                // Cleanup on error
                if (File.Exists(tempNestedArchive))
                {
                    try { File.Delete(tempNestedArchive); } catch { }
                }
                throw;
            }
        }
    }

    private Stream ExtractFileFromArchive(string archivePath, string filePathInArchive)
    {
        IArchive? archive = null;

        if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            archive = ZipArchive.Open(archivePath);
        }
        else if (archivePath.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
        {
            archive = SevenZipArchive.Open(archivePath);
        }
        else if (archivePath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase))
        {
            archive = RarArchive.Open(archivePath);
        }

        if (archive == null)
            throw new InvalidOperationException($"Unsupported archive format: {archivePath}");

        var entry = archive.Entries.FirstOrDefault(e => e.Key == filePathInArchive);
        if (entry == null)
            throw new FileNotFoundException($"File {filePathInArchive} not found in archive {archivePath}");

        var memoryStream = new MemoryStream();
        using (var entryStream = entry.OpenEntryStream())
        {
            entryStream.CopyTo(memoryStream);
        }
        memoryStream.Position = 0;

        archive.Dispose();
        return memoryStream;
    }

    private static bool IsArchiveFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".7z", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".rar", StringComparison.OrdinalIgnoreCase);
    }
}

