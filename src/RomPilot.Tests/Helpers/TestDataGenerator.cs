using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RomPilot.Tests.Helpers;

/// <summary>
/// Générateur de fichiers de test avec hashs maîtrisés pour US1.
/// Génère des fichiers de test dans différents scénarios (simple, moyen, charge).
/// </summary>
public class TestDataGenerator
{
    private readonly string _baseDirectory;

    public TestDataGenerator(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    /// <summary>
    /// Génère un scénario simple (comme "mixed" actuel) - ~10 fichiers
    /// </summary>
    public async Task GenerateSimpleScenarioAsync()
    {
        var scenarioDir = Path.Combine(_baseDirectory, "simple");
        Directory.CreateDirectory(scenarioDir);

        // ROMs variées avec différentes extensions
        await CreateRomFileAsync(Path.Combine(scenarioDir, "game1.nes"), "NES_ROM_001", 1024, "nes");
        await CreateRomFileAsync(Path.Combine(scenarioDir, "game2.smc"), "SNES_ROM_001", 2048, "smc");
        await CreateRomFileAsync(Path.Combine(scenarioDir, "game3.iso"), "PSX_ROM_001", 4096, "iso");

        // Fichiers à exclure
        await CreateTextFileAsync(Path.Combine(scenarioDir, "cover.jpg"), "FAKE_JPEG_CONTENT");
        await CreateTextFileAsync(Path.Combine(scenarioDir, "readme.txt"), "This is a readme file");
        await CreateTextFileAsync(Path.Combine(scenarioDir, "manual.pdf"), "FAKE_PDF_CONTENT");

        // Archives simples (ZIP, 7Z, RAR)
        var archiveDir = Path.Combine(scenarioDir, "archives");
        Directory.CreateDirectory(archiveDir);
        var romInZip = Path.Combine(archiveDir, "game_zip.nes");
        await CreateRomFileAsync(romInZip, "ZIP_ROM_001", 1536, "nes");
        CreateZipArchive(Path.Combine(archiveDir, "games.zip"), new[] { romInZip });
        File.Delete(romInZip);
        
        var romIn7z = Path.Combine(archiveDir, "game_7z.smc");
        await CreateRomFileAsync(romIn7z, "7Z_ROM_001", 2048, "smc");
        Create7zArchive(Path.Combine(archiveDir, "games.7z"), new[] { romIn7z });
        File.Delete(romIn7z);
        
        var romInRar = Path.Combine(archiveDir, "game_rar.iso");
        await CreateRomFileAsync(romInRar, "RAR_ROM_001", 3072, "iso");
        CreateRarArchive(Path.Combine(archiveDir, "games.rar"), new[] { romInRar });
        File.Delete(romInRar);

        Console.WriteLine($"✅ Scénario simple créé dans {scenarioDir}");
    }

    /// <summary>
    /// Génère un scénario moyen - ~50 fichiers avec archives imbriquées
    /// </summary>
    public async Task GenerateMediumScenarioAsync()
    {
        var scenarioDir = Path.Combine(_baseDirectory, "medium");
        Directory.CreateDirectory(scenarioDir);

        // ROMs variées avec différentes extensions et consoles
        var romExtensions = new[] { ".nes", ".smc", ".iso", ".gb", ".gba", ".nds", ".bin", ".cue" };
        var romHeaders = new Dictionary<string, byte[]>
        {
            { ".nes", new byte[] { 0x4E, 0x45, 0x53, 0x1A } },
            { ".smc", new byte[] { 0x00, 0xFF, 0xFF, 0xFF } }, // SNES
            { ".iso", new byte[] { 0x01, 0x43, 0x44, 0x30, 0x30, 0x31 } }, // ISO9660
            { ".gb", new byte[] { 0xCE, 0xED, 0x66, 0x66 } }, // Game Boy
            { ".gba", new byte[] { 0x24, 0xFF, 0xAE, 0x51 } }, // GBA
            { ".nds", new byte[] { 0x4E, 0x44, 0x53 } }, // Nintendo DS
            { ".bin", new byte[] { 0x00, 0x00, 0x00, 0x00 } }, // Generic binary
            { ".cue", Array.Empty<byte>() } // CUE file (text)
        };
        
        for (int i = 1; i <= 10; i++)
        {
            var ext = romExtensions[i % romExtensions.Length];
            var header = romHeaders[ext];
            await CreateRomFileWithHeaderAsync(
                Path.Combine(scenarioDir, $"game{i:D2}{ext}"),
                $"ROM_{i:D3}",
                1024 + (i * 128),
                header);
        }

        // Fichiers à exclure variés
        var excludedExtensions = new[] { ".jpg", ".png", ".txt", ".pdf", ".nfo", ".diz", ".exe" };
        for (int i = 0; i < excludedExtensions.Length; i++)
        {
            await CreateTextFileAsync(
                Path.Combine(scenarioDir, $"file{i + 1}{excludedExtensions[i]}"),
                $"FAKE_CONTENT_{excludedExtensions[i]}");
        }

        // Archive ZIP avec ROMs
        var archive1Dir = Path.Combine(scenarioDir, "archive1");
        Directory.CreateDirectory(archive1Dir);
        var romsInArchive1 = new List<string>();
        for (int i = 1; i <= 5; i++)
        {
            var ext = romExtensions[i % romExtensions.Length];
            var header = romHeaders[ext];
            var rom = Path.Combine(archive1Dir, $"archive_game{i}{ext}");
            await CreateRomFileWithHeaderAsync(rom, $"ARCH1_ROM_{i:D3}", 2048, header);
            romsInArchive1.Add(rom);
        }
        CreateZipArchive(Path.Combine(scenarioDir, "archive1.zip"), romsInArchive1);
        Directory.Delete(archive1Dir, recursive: true);
        
        // Archive 7Z avec ROMs
        var archive7zDir = Path.Combine(scenarioDir, "archive7z");
        Directory.CreateDirectory(archive7zDir);
        var romsIn7z = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            var ext = romExtensions[(i + 2) % romExtensions.Length];
            var header = romHeaders[ext];
            var rom = Path.Combine(archive7zDir, $"archive7z_game{i}{ext}");
            await CreateRomFileWithHeaderAsync(rom, $"7Z_ROM_{i:D3}", 2560, header);
            romsIn7z.Add(rom);
        }
        Create7zArchive(Path.Combine(scenarioDir, "archive7z.7z"), romsIn7z);
        Directory.Delete(archive7zDir, recursive: true);
        
        // Archive RAR avec ROMs
        var archiveRarDir = Path.Combine(scenarioDir, "archiveRar");
        Directory.CreateDirectory(archiveRarDir);
        var romsInRar = new List<string>();
        for (int i = 1; i <= 3; i++)
        {
            var ext = romExtensions[(i + 4) % romExtensions.Length];
            var header = romHeaders[ext];
            var rom = Path.Combine(archiveRarDir, $"archiveRar_game{i}{ext}");
            await CreateRomFileWithHeaderAsync(rom, $"RAR_ROM_{i:D3}", 3072, header);
            romsInRar.Add(rom);
        }
        CreateRarArchive(Path.Combine(scenarioDir, "archiveRar.rar"), romsInRar);
        Directory.Delete(archiveRarDir, recursive: true);

        // Archives imbriquées 2 niveaux avec formats mixtes
        // ZIP dans ZIP
        var nestedZipDir = Path.Combine(scenarioDir, "nestedZip");
        Directory.CreateDirectory(nestedZipDir);
        var innerZipDir = Path.Combine(nestedZipDir, "inner");
        Directory.CreateDirectory(innerZipDir);
        var romInInnerZip = Path.Combine(innerZipDir, "nested_game.nes");
        await CreateRomFileWithHeaderAsync(romInInnerZip, "NESTED_ROM_001", 3072, romHeaders[".nes"]);
        var innerZipArchive = Path.Combine(nestedZipDir, "inner.zip");
        CreateZipArchive(innerZipArchive, new[] { romInInnerZip });
        File.Delete(romInInnerZip);
        Directory.Delete(innerZipDir);
        CreateZipArchive(Path.Combine(scenarioDir, "nested.zip"), new[] { innerZipArchive });
        Directory.Delete(nestedZipDir, recursive: true);
        
        // 7Z dans ZIP
        var nested7zDir = Path.Combine(scenarioDir, "nested7z");
        Directory.CreateDirectory(nested7zDir);
        var inner7zDir = Path.Combine(nested7zDir, "inner");
        Directory.CreateDirectory(inner7zDir);
        var romInInner7z = Path.Combine(inner7zDir, "nested7z_game.smc");
        await CreateRomFileWithHeaderAsync(romInInner7z, "NESTED7Z_ROM_001", 3584, romHeaders[".smc"]);
        var inner7zArchive = Path.Combine(nested7zDir, "inner.7z");
        Create7zArchive(inner7zArchive, new[] { romInInner7z });
        File.Delete(romInInner7z);
        Directory.Delete(inner7zDir);
        CreateZipArchive(Path.Combine(scenarioDir, "nested7z.zip"), new[] { inner7zArchive });
        Directory.Delete(nested7zDir, recursive: true);
        
        // RAR dans 7Z
        var nestedRarDir = Path.Combine(scenarioDir, "nestedRar");
        Directory.CreateDirectory(nestedRarDir);
        var innerRarDir = Path.Combine(nestedRarDir, "inner");
        Directory.CreateDirectory(innerRarDir);
        var romInInnerRar = Path.Combine(innerRarDir, "nestedRar_game.iso");
        await CreateRomFileWithHeaderAsync(romInInnerRar, "NESTEDRAR_ROM_001", 4096, romHeaders[".iso"]);
        var innerRarArchive = Path.Combine(nestedRarDir, "inner.rar");
        CreateRarArchive(innerRarArchive, new[] { romInInnerRar });
        File.Delete(romInInnerRar);
        Directory.Delete(innerRarDir);
        Create7zArchive(Path.Combine(scenarioDir, "nestedRar.7z"), new[] { innerRarArchive });
        Directory.Delete(nestedRarDir, recursive: true);
        
        // Archive imbriquée 3 niveaux avec formats mixtes (ZIP dans 7Z dans RAR)
        var deepNestedDir = Path.Combine(scenarioDir, "deep");
        Directory.CreateDirectory(deepNestedDir);
        var level2Dir = Path.Combine(deepNestedDir, "level2");
        Directory.CreateDirectory(level2Dir);
        var level3Dir = Path.Combine(level2Dir, "level3");
        Directory.CreateDirectory(level3Dir);
        var romInLevel3 = Path.Combine(level3Dir, "deep_game.gba");
        await CreateRomFileWithHeaderAsync(romInLevel3, "DEEP_ROM_001", 5120, romHeaders[".gba"]);
        var level3Archive = Path.Combine(level2Dir, "level3.zip");
        CreateZipArchive(level3Archive, new[] { romInLevel3 });
        File.Delete(romInLevel3);
        Directory.Delete(level3Dir);
        var level2Archive = Path.Combine(deepNestedDir, "level2.7z");
        Create7zArchive(level2Archive, new[] { level3Archive });
        File.Delete(level3Archive);
        Directory.Delete(level2Dir);
        CreateRarArchive(Path.Combine(scenarioDir, "deep.rar"), new[] { level2Archive });
        File.Delete(level2Archive);
        Directory.Delete(deepNestedDir, recursive: true);

        Console.WriteLine($"✅ Scénario moyen créé dans {scenarioDir}");
    }

    /// <summary>
    /// Génère un scénario de charge - ~500 fichiers pour tester les performances
    /// </summary>
    public async Task GenerateLoadScenarioAsync()
    {
        var scenarioDir = Path.Combine(_baseDirectory, "load");
        Directory.CreateDirectory(scenarioDir);

        // Beaucoup de ROMs avec extensions variées
        var romExtensions = new[] { ".nes", ".smc", ".iso", ".gb", ".gba", ".nds", ".bin", ".cue" };
        var romHeaders = new Dictionary<string, byte[]>
        {
            { ".nes", new byte[] { 0x4E, 0x45, 0x53, 0x1A } },
            { ".smc", new byte[] { 0x00, 0xFF, 0xFF, 0xFF } },
            { ".iso", new byte[] { 0x01, 0x43, 0x44, 0x30, 0x30, 0x31 } },
            { ".gb", new byte[] { 0xCE, 0xED, 0x66, 0x66 } },
            { ".gba", new byte[] { 0x24, 0xFF, 0xAE, 0x51 } },
            { ".nds", new byte[] { 0x4E, 0x44, 0x53 } },
            { ".bin", new byte[] { 0x00, 0x00, 0x00, 0x00 } },
            { ".cue", Array.Empty<byte>() }
        };
        
        for (int i = 1; i <= 100; i++)
        {
            var ext = romExtensions[i % romExtensions.Length];
            var header = romHeaders[ext];
            await CreateRomFileWithHeaderAsync(
                Path.Combine(scenarioDir, $"rom_{i:D3}{ext}"),
                $"LOAD_ROM_{i:D3}",
                1024 + (i * 64),
                header);
        }

        // Beaucoup de fichiers à exclure
        var excludedTypes = new[] { ".jpg", ".png", ".txt", ".pdf", ".nfo", ".diz", ".exe", ".dll", ".html", ".xml" };
        for (int i = 1; i <= 100; i++)
        {
            var ext = excludedTypes[i % excludedTypes.Length];
            await CreateTextFileAsync(
                Path.Combine(scenarioDir, $"excluded_{i:D3}{ext}"),
                $"FAKE_{ext.ToUpper()}_CONTENT_{i}");
        }

        // Plusieurs archives avec beaucoup de fichiers
        for (int archiveNum = 1; archiveNum <= 10; archiveNum++)
        {
            var archiveDir = Path.Combine(scenarioDir, $"archive{archiveNum}");
            Directory.CreateDirectory(archiveDir);
            var roms = new List<string>();
            
            for (int i = 1; i <= 20; i++)
            {
                var rom = Path.Combine(archiveDir, $"arch{archiveNum}_rom{i:D2}.nes");
                await CreateRomFileAsync(rom, $"ARCH{archiveNum}_ROM_{i:D3}", 1536);
                roms.Add(rom);
            }
            
            CreateZipArchive(Path.Combine(scenarioDir, $"archive{archiveNum}.zip"), roms);
            Directory.Delete(archiveDir, recursive: true);
        }

        // Archives imbriquées multiples
        for (int nestedNum = 1; nestedNum <= 5; nestedNum++)
        {
            var nestedDir = Path.Combine(scenarioDir, $"nested{nestedNum}");
            Directory.CreateDirectory(nestedDir);
            
            // Archive interne
            var innerDir = Path.Combine(nestedDir, "inner");
            Directory.CreateDirectory(innerDir);
            var innerRoms = new List<string>();
            for (int i = 1; i <= 5; i++)
            {
                var rom = Path.Combine(innerDir, $"nested{nestedNum}_inner{i}.nes");
                await CreateRomFileAsync(rom, $"NEST{nestedNum}_INNER_{i}", 2048);
                innerRoms.Add(rom);
            }
            var innerArchive = Path.Combine(nestedDir, "inner.zip");
            CreateZipArchive(innerArchive, innerRoms);
            Directory.Delete(innerDir, recursive: true);
            
            // Archive externe
            CreateZipArchive(Path.Combine(scenarioDir, $"nested{nestedNum}.zip"), new[] { innerArchive });
            Directory.Delete(nestedDir, recursive: true);
        }

        Console.WriteLine($"✅ Scénario de charge créé dans {scenarioDir}");
    }

    /// <summary>
    /// Crée un fichier ROM avec un contenu maîtrisé pour avoir un hash prévisible
    /// </summary>
    private async Task CreateRomFileAsync(string filePath, string content, int size, string extension = "nes")
    {
        var header = extension switch
        {
            "nes" => new byte[] { 0x4E, 0x45, 0x53, 0x1A },
            "smc" => new byte[] { 0x00, 0xFF, 0xFF, 0xFF },
            "iso" => new byte[] { 0x01, 0x43, 0x44, 0x30, 0x30, 0x31 },
            "gb" => new byte[] { 0xCE, 0xED, 0x66, 0x66 },
            "gba" => new byte[] { 0x24, 0xFF, 0xAE, 0x51 },
            "nds" => new byte[] { 0x4E, 0x44, 0x53 },
            "bin" => new byte[] { 0x00, 0x00, 0x00, 0x00 },
            "cue" => Array.Empty<byte>(),
            _ => new byte[] { 0x00, 0x00, 0x00, 0x00 }
        };
        
        await CreateRomFileWithHeaderAsync(filePath, content, size, header);
    }
    
    /// <summary>
    /// Crée un fichier ROM avec un header spécifique
    /// </summary>
    private async Task CreateRomFileWithHeaderAsync(string filePath, string content, int size, byte[] header)
    {
        // Contenu basé sur le seed pour avoir un hash maîtrisé
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var totalHeaderSize = Math.Max(header.Length, 4); // Minimum 4 bytes
        var padding = new byte[Math.Max(0, size - totalHeaderSize - contentBytes.Length)];
        Array.Fill(padding, (byte)0x00);
        
        var fullContent = new byte[totalHeaderSize + contentBytes.Length + padding.Length];
        if (header.Length > 0)
        {
            Buffer.BlockCopy(header, 0, fullContent, 0, Math.Min(header.Length, totalHeaderSize));
        }
        Buffer.BlockCopy(contentBytes, 0, fullContent, totalHeaderSize, contentBytes.Length);
        if (padding.Length > 0)
        {
            Buffer.BlockCopy(padding, 0, fullContent, totalHeaderSize + contentBytes.Length, padding.Length);
        }
        
        await File.WriteAllBytesAsync(filePath, fullContent);
    }

    /// <summary>
    /// Crée un fichier texte simple
    /// </summary>
    private async Task CreateTextFileAsync(string filePath, string content)
    {
        await File.WriteAllTextAsync(filePath, content);
    }

    /// <summary>
    /// Crée une archive ZIP avec les fichiers spécifiés
    /// </summary>
    private void CreateZipArchive(string zipPath, IEnumerable<string> files)
    {
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    var entryName = Path.GetFileName(file);
                    zip.CreateEntryFromFile(file, entryName);
                }
            }
        }
    }
    
    /// <summary>
    /// Crée une archive 7Z avec les fichiers spécifiés
    /// Utilise la commande 7z (doit être installé dans le dev container)
    /// </summary>
    private void Create7zArchive(string archivePath, IEnumerable<string> files)
    {
        if (!CommandExists("7z") && !CommandExists("7za"))
        {
            throw new InvalidOperationException("7z command not found. Please install p7zip-full in the dev container.");
        }
        
        Create7zArchiveWithCommandLine(archivePath, files);
    }
    
    /// <summary>
    /// Crée une archive RAR avec les fichiers spécifiés
    /// Utilise la commande rar ou unrar (doit être installé dans le dev container)
    /// </summary>
    private void CreateRarArchive(string archivePath, IEnumerable<string> files)
    {
        if (!CommandExists("rar") && !CommandExists("unrar"))
        {
            throw new InvalidOperationException("rar/unrar command not found. Please install unrar or rar in the dev container.");
        }
        
        CreateRarArchiveWithCommandLine(archivePath, files);
    }
    
    private void Create7zArchiveWithCommandLine(string archivePath, IEnumerable<string> files)
    {
        var command = CommandExists("7z") ? "7z" : "7za";
        var fileList = string.Join(" ", files.Where(File.Exists).Select(f => $"\"{f}\""));
        
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = command,
            Arguments = $"a -t7z \"{archivePath}\" {fileList}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });
        process?.WaitForExit();
        
        if (process?.ExitCode != 0 || !File.Exists(archivePath))
        {
            var error = process?.StandardError?.ReadToEnd() ?? "Unknown error";
            throw new Exception($"7z command failed with exit code {process?.ExitCode}: {error}");
        }
    }
    
    private void CreateRarArchiveWithCommandLine(string archivePath, IEnumerable<string> files)
    {
        // Note: unrar peut décompresser mais PAS créer. rar (non-libre) peut créer.
        // Si rar n'est pas disponible, on ne peut pas créer d'archives RAR
        if (!CommandExists("rar"))
        {
            Console.WriteLine($"Warning: rar command not found. Skipping RAR archive creation for {archivePath}. Install rar (non-free) to create RAR archives.");
            // Ne pas créer l'archive - les tests qui en dépendent échoueront proprement
            return;
        }
        
        var fileList = string.Join(" ", files.Where(File.Exists).Select(f => $"\"{f}\""));
        var arguments = $"a -ep \"{archivePath}\" {fileList}";
        
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "rar",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });
        process?.WaitForExit();
        
        if (process?.ExitCode != 0 || !File.Exists(archivePath))
        {
            var error = process?.StandardError?.ReadToEnd() ?? process?.StandardOutput?.ReadToEnd() ?? "Unknown error";
            throw new Exception($"rar command failed with exit code {process?.ExitCode}: {error}");
        }
    }
    
    private bool CommandExists(string command)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calcule et affiche les hashs des fichiers générés pour vérification
    /// </summary>
    public async Task PrintFileHashesAsync(string scenarioDir)
    {
        Console.WriteLine($"\n📊 Hashs des fichiers dans {scenarioDir}:");
        Console.WriteLine("=" .PadRight(80, '='));
        
        var files = Directory.GetFiles(scenarioDir, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f);
        
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(scenarioDir, file);
            var md5 = await CalculateMD5Async(file);
            var sha1 = await CalculateSHA1Async(file);
            var fileInfo = new FileInfo(file);
            Console.WriteLine($"{relativePath,-50} | MD5: {md5} | SHA1: {sha1.Substring(0, 16)}... | {fileInfo.Length} bytes");
        }
    }

    private async Task<string> CalculateMD5Async(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    private async Task<string> CalculateSHA1Async(string filePath)
    {
        using (var sha1 = SHA1.Create())
        using (var stream = File.OpenRead(filePath))
        {
            var hash = await sha1.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}

