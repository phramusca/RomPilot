using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RomPilot.TestData;

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

        // ROMs NES avec hashs maîtrisés
        await CreateRomFileAsync(Path.Combine(scenarioDir, "game1.nes"), "NES_ROM_001", 1024);
        await CreateRomFileAsync(Path.Combine(scenarioDir, "game2.nes"), "NES_ROM_002", 2048);

        // Fichiers à exclure
        await CreateTextFileAsync(Path.Combine(scenarioDir, "cover.jpg"), "FAKE_JPEG_CONTENT");
        await CreateTextFileAsync(Path.Combine(scenarioDir, "readme.txt"), "This is a readme file");
        await CreateTextFileAsync(Path.Combine(scenarioDir, "manual.pdf"), "FAKE_PDF_CONTENT");

        // Archive simple
        var archiveDir = Path.Combine(scenarioDir, "archives");
        Directory.CreateDirectory(archiveDir);
        var romInArchive = Path.Combine(archiveDir, "game3.nes");
        await CreateRomFileAsync(romInArchive, "NES_ROM_003", 1536);
        CreateZipArchive(Path.Combine(archiveDir, "games.zip"), new[] { romInArchive });

        Console.WriteLine($"✅ Scénario simple créé dans {scenarioDir}");
    }

    /// <summary>
    /// Génère un scénario moyen - ~50 fichiers avec archives imbriquées
    /// </summary>
    public async Task GenerateMediumScenarioAsync()
    {
        var scenarioDir = Path.Combine(_baseDirectory, "medium");
        Directory.CreateDirectory(scenarioDir);

        // ROMs variées
        for (int i = 1; i <= 10; i++)
        {
            await CreateRomFileAsync(
                Path.Combine(scenarioDir, $"game{i:D2}.nes"),
                $"NES_ROM_{i:D3}",
                1024 + (i * 128));
        }

        // Fichiers à exclure variés
        var excludedExtensions = new[] { ".jpg", ".png", ".txt", ".pdf", ".nfo", ".diz", ".exe" };
        for (int i = 0; i < excludedExtensions.Length; i++)
        {
            await CreateTextFileAsync(
                Path.Combine(scenarioDir, $"file{i + 1}{excludedExtensions[i]}"),
                $"FAKE_CONTENT_{excludedExtensions[i]}");
        }

        // Archive avec ROMs
        var archive1Dir = Path.Combine(scenarioDir, "archive1");
        Directory.CreateDirectory(archive1Dir);
        var romsInArchive1 = new List<string>();
        for (int i = 1; i <= 5; i++)
        {
            var rom = Path.Combine(archive1Dir, $"archive_game{i}.nes");
            await CreateRomFileAsync(rom, $"ARCH1_ROM_{i:D3}", 2048);
            romsInArchive1.Add(rom);
        }
        CreateZipArchive(Path.Combine(scenarioDir, "archive1.zip"), romsInArchive1);

        // Archive imbriquée (archive dans archive)
        var nestedDir = Path.Combine(scenarioDir, "nested");
        Directory.CreateDirectory(nestedDir);
        var innerArchiveDir = Path.Combine(nestedDir, "inner");
        Directory.CreateDirectory(innerArchiveDir);
        var romInInner = Path.Combine(innerArchiveDir, "nested_game.nes");
        await CreateRomFileAsync(romInInner, "NESTED_ROM_001", 3072);
        var innerArchive = Path.Combine(nestedDir, "inner.zip");
        CreateZipArchive(innerArchive, new[] { romInInner });

        // Archive externe contenant l'archive interne
        CreateZipArchive(Path.Combine(scenarioDir, "nested.zip"), new[] { innerArchive });

        Console.WriteLine($"✅ Scénario moyen créé dans {scenarioDir}");
    }

    /// <summary>
    /// Génère un scénario de charge - ~500 fichiers pour tester les performances
    /// </summary>
    public async Task GenerateLoadScenarioAsync()
    {
        var scenarioDir = Path.Combine(_baseDirectory, "load");
        Directory.CreateDirectory(scenarioDir);

        // Beaucoup de ROMs
        for (int i = 1; i <= 100; i++)
        {
            await CreateRomFileAsync(
                Path.Combine(scenarioDir, $"rom_{i:D3}.nes"),
                $"LOAD_ROM_{i:D3}",
                1024 + (i * 64));
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

            // Archive externe
            CreateZipArchive(Path.Combine(scenarioDir, $"nested{nestedNum}.zip"), new[] { innerArchive });
        }

        Console.WriteLine($"✅ Scénario de charge créé dans {scenarioDir}");
    }

    /// <summary>
    /// Crée un fichier ROM avec un contenu maîtrisé pour avoir un hash prévisible
    /// </summary>
    private async Task CreateRomFileAsync(string filePath, string content, int size)
    {
        // Header NES standard
        var header = new byte[] { 0x4E, 0x45, 0x53, 0x1A };

        // Contenu basé sur le seed pour avoir un hash maîtrisé
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var padding = new byte[size - header.Length - contentBytes.Length];
        Array.Fill(padding, (byte)0x00);

        var fullContent = new byte[header.Length + contentBytes.Length + padding.Length];
        Buffer.BlockCopy(header, 0, fullContent, 0, header.Length);
        Buffer.BlockCopy(contentBytes, 0, fullContent, header.Length, contentBytes.Length);
        Buffer.BlockCopy(padding, 0, fullContent, header.Length + contentBytes.Length, padding.Length);

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
    /// Calcule et affiche les hashs des fichiers générés pour vérification
    /// </summary>
    public async Task PrintFileHashesAsync(string scenarioDir)
    {
        Console.WriteLine($"\n📊 Hashs des fichiers dans {scenarioDir}:");
        Console.WriteLine("=".PadRight(80, '='));

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

