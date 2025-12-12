using System.Xml.Linq;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.DatabaseProviders;

/// <summary>
/// Provider for parsing Redump datfiles (XML format).
/// </summary>
public class RedumpProvider : IRedumpProvider
{
    private readonly RomPilotDbContext _context;

    public RedumpProvider(RomPilotDbContext context)
    {
        _context = context;
    }

    public string Name => "Redump";

    public bool IsValidDatfile(string datfilePath)
    {
        if (!File.Exists(datfilePath))
            return false;

        try
        {
            var doc = XDocument.Load(datfilePath);
            var root = doc.Root;
            // Redump uses <datafile> with <game> and <rom> elements
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
            throw new ArgumentException($"Invalid Redump datfile: {datfilePath}");

        var gameEntries = new List<GameEntry>();

        await Task.Run(() =>
        {
            var doc = XDocument.Load(datfilePath);
            var root = doc.Root;
            if (root == null)
                return;

            // Redump datfile structure: <datafile><game><rom .../></game></datafile>
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

                    // Redump typically uses MD5 or SHA1
                    if (!string.IsNullOrEmpty(md5))
                    {
                        var entry = new GameEntry
                        {
                            ReferenceDatabaseId = databaseSourceId,
                            GameName = gameName,
                            HashType = "MD5",
                            HashValue = md5,
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
                }
            }
        }, cancellationToken);

        return gameEntries;
    }
}

