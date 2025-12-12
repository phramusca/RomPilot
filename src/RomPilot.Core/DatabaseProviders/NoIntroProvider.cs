using System.Xml.Linq;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.DatabaseProviders;

/// <summary>
/// Provider for parsing NoIntro datfiles (XML format).
/// </summary>
public class NoIntroProvider : INoIntroProvider
{
    private readonly RomPilotDbContext _context;

    public NoIntroProvider(RomPilotDbContext context)
    {
        _context = context;
    }

    public string Name => "NoIntro";

    public bool IsValidDatfile(string datfilePath)
    {
        if (!File.Exists(datfilePath))
            return false;

        try
        {
            var doc = XDocument.Load(datfilePath);
            return doc.Root?.Name.LocalName == "datafile" || 
                   doc.Root?.Attribute("version") != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<GameEntry>> LoadGameEntriesAsync(
        string datfilePath,
        int databaseSourceId,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidDatfile(datfilePath))
            throw new ArgumentException($"Invalid NoIntro datfile: {datfilePath}");

        var gameEntries = new List<GameEntry>();

        await Task.Run(() =>
        {
            var doc = XDocument.Load(datfilePath);
            var root = doc.Root;
            if (root == null)
                return;

            // NoIntro datfile structure: <datafile><game><rom .../></game></datafile>
            var games = root.Descendants("game");
            
            foreach (var game in games)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var gameName = game.Attribute("name")?.Value ?? string.Empty;
                var roms = game.Descendants("rom");

                foreach (var rom in roms)
                {
                    var name = rom.Attribute("name")?.Value ?? string.Empty;
                    var md5 = rom.Attribute("md5")?.Value?.ToUpperInvariant();
                    var sha1 = rom.Attribute("sha1")?.Value?.ToUpperInvariant();
                    var crc = rom.Attribute("crc")?.Value?.ToUpperInvariant();
                    var size = rom.Attribute("size")?.Value;

                    // Try to detect console from game name or header
                    // For now, we'll need to match with existing consoles
                    // This is a simplified version - full implementation would parse console info from datfile
                    
                    if (!string.IsNullOrEmpty(md5))
                    {
                        var entry = new GameEntry
                        {
                            ReferenceDatabaseId = databaseSourceId,
                            GameName = gameName,
                            HashType = "MD5",
                            HashValue = md5,
                            // ConsoleId will need to be determined from the datfile or matched
                        };
                        gameEntries.Add(entry);
                    }
                    else if (!string.IsNullOrEmpty(sha1))
                    {
                        var entry = new GameEntry
                        {
                            ReferenceDatabaseId = databaseSourceId,
                            GameName = gameName,
                            HashType = "SHA1",
                            HashValue = sha1,
                        };
                        gameEntries.Add(entry);
                    }
                    else if (!string.IsNullOrEmpty(crc))
                    {
                        var entry = new GameEntry
                        {
                            ReferenceDatabaseId = databaseSourceId,
                            GameName = gameName,
                            HashType = "CRC32",
                            HashValue = crc,
                        };
                        gameEntries.Add(entry);
                    }
                }
            }
        }, cancellationToken);

        return gameEntries;
    }
}

