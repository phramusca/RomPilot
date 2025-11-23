using System.Xml.Linq;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.DatabaseProviders;

/// <summary>
/// Provider for parsing GoodSet datfiles (XML format).
/// </summary>
public class GoodSetProvider : IGoodSetProvider
{
    private readonly RomPilotDbContext _context;

    public GoodSetProvider(RomPilotDbContext context)
    {
        _context = context;
    }

    public string Name => "GoodSet";

    public bool IsValidDatfile(string datfilePath)
    {
        if (!File.Exists(datfilePath))
            return false;

        try
        {
            var doc = XDocument.Load(datfilePath);
            var root = doc.Root;
            // GoodSet uses similar structure to NoIntro/Redump
            return root?.Name.LocalName == "datafile";
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
            throw new ArgumentException($"Invalid GoodSet datfile: {datfilePath}");

        var gameEntries = new List<GameEntry>();

        await Task.Run(() =>
        {
            var doc = XDocument.Load(datfilePath);
            var root = doc.Root;
            if (root == null)
                return;

            // GoodSet datfile structure: <datafile><game><rom .../></game></datafile>
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

                    // GoodSet typically uses MD5 or CRC32
                    if (!string.IsNullOrEmpty(md5))
                    {
                        var entry = new GameEntry
                        {
                            DatabaseSourceId = databaseSourceId,
                            GameName = gameName,
                            HashType = "MD5",
                            HashValue = md5,
                        };
                        gameEntries.Add(entry);
                    }
                    else if (!string.IsNullOrEmpty(crc))
                    {
                        var entry = new GameEntry
                        {
                            DatabaseSourceId = databaseSourceId,
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

