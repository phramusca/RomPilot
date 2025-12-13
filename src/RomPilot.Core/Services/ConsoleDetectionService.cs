using RomPilot.Core.Repositories;

namespace RomPilot.Core.Services;

/// <summary>
/// Implementation of console detection service.
/// Detects console based on file extension, size, and file headers.
/// </summary>
public class ConsoleDetectionService : IConsoleDetectionService
{
    private readonly IConsoleRepository _consoleRepository;

    // Console detection mappings: extension -> console short name
    private static readonly Dictionary<string, string> ExtensionToConsole = new(StringComparer.OrdinalIgnoreCase)
    {
        // Nintendo
        { ".nes", "nes" },
        { ".snes", "snes" },
        { ".smc", "snes" },
        { ".sfc", "snes" },
        { ".gb", "gb" },
        { ".gbc", "gbc" },
        { ".gba", "gba" },
        { ".n64", "n64" },
        { ".z64", "n64" },
        { ".v64", "n64" },
        { ".nds", "nds" },
        { ".dsi", "nds" },
        { ".3ds", "3ds" },
        { ".cia", "3ds" },
        { ".cci", "3ds" },
        
        // Sega
        { ".md", "megadrive" },
        { ".gen", "megadrive" },
        { ".sms", "mastersystem" },
        { ".gg", "gamegear" },
        { ".32x", "sega32x" },
        { ".smd", "megadrive" },
        
        // Sony
        { ".psx", "psx" },
        { ".bin", "psx" },
        { ".cue", "psx" },
        { ".iso", "psx" },
        { ".img", "psx" },
        { ".mdf", "psx" },
        { ".chd", "psx" },
        { ".psp", "psp" },
        
        // Atari
        { ".a26", "atari2600" },
        { ".a52", "atari5200" },
        { ".a78", "atari7800" },
        { ".lynx", "lynx" },
        { ".jag", "jaguar" },
        
        // NEC
        { ".pce", "pcengine" },
        { ".ngp", "ngp" },
        { ".ngc", "ngpc" },
        
        // SNK
        { ".ng", "neogeo" },
        
        // Other
        { ".ws", "ws" },
        { ".wsc", "wsc" },
        { ".vb", "virtualboy" },
        { ".rom", "c64" },
    };

    public ConsoleDetectionService(IConsoleRepository consoleRepository)
    {
        _consoleRepository = consoleRepository;
    }

    public async Task<string?> DetectConsoleAsync(string filePath, long fileSize, Stream? fileStream = null)
    {
        // First, try detection by file extension
        var extension = Path.GetExtension(filePath);
        System.Console.WriteLine($"[ConsoleDetectionService] Detecting console for: {filePath}, extension: '{extension}'");
        if (!string.IsNullOrEmpty(extension) && ExtensionToConsole.TryGetValue(extension, out var consoleShortName))
        {
            System.Console.WriteLine($"[ConsoleDetectionService] Extension '{extension}' maps to console '{consoleShortName}'");
            // Verify console exists in database
            var console = await _consoleRepository.GetByShortNameAsync(consoleShortName);
            if (console != null)
            {
                System.Console.WriteLine($"[ConsoleDetectionService] Console '{consoleShortName}' found in database: {console.Name}");
                return consoleShortName;
            }
            else
            {
                System.Console.WriteLine($"[ConsoleDetectionService] Console '{consoleShortName}' NOT found in database");
            }
        }
        else
        {
            System.Console.WriteLine($"[ConsoleDetectionService] Extension '{extension}' not recognized or empty");
        }

        // TODO: Add header-based detection for ambiguous cases
        // This would read file headers to identify console-specific signatures

        return null;
    }
}

