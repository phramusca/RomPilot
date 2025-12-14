using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Database;

/// <summary>
/// Service for seeding initial database data (consoles, database sources, exclusion filters).
/// </summary>
public class SeedDataService
{
    private readonly RomPilotDbContext _context;

    public SeedDataService(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if already seeded
        var alreadySeeded = await _context.Consoles.AnyAsync();
        
        if (alreadySeeded)
        {
            // Even if consoles are seeded, check if exclusion filters need to be seeded
            await SeedExclusionFiltersIfNeededAsync();
            return; // Already seeded
        }

        // Seed consoles (35+ retro gaming consoles)
        var consoles = new List<Models.Console>
        {
            new() { Name = "Nintendo Entertainment System", ShortName = "nes", RecalboxFolderName = "nes", SupportedFormats = "[\".nes\", \".smc\"]", ExportFormat = "ZIP" },
            new() { Name = "Super Nintendo", ShortName = "snes", RecalboxFolderName = "snes", SupportedFormats = "[\".snes\", \".smc\", \".sfc\"]", ExportFormat = "ZIP" },
            new() { Name = "Nintendo Game Boy", ShortName = "gb", RecalboxFolderName = "gb", SupportedFormats = "[\".gb\"]", ExportFormat = "ZIP" },
            new() { Name = "Nintendo Game Boy Color", ShortName = "gbc", RecalboxFolderName = "gbc", SupportedFormats = "[\".gbc\"]", ExportFormat = "ZIP" },
            new() { Name = "Nintendo Game Boy Advance", ShortName = "gba", RecalboxFolderName = "gba", SupportedFormats = "[\".gba\"]", ExportFormat = "ZIP" },
            new() { Name = "Nintendo 64", ShortName = "n64", RecalboxFolderName = "n64", SupportedFormats = "[\".n64\", \".z64\", \".v64\"]", ExportFormat = "ZIP" },
            new() { Name = "Nintendo Virtual Boy", ShortName = "virtualboy", RecalboxFolderName = "virtualboy", SupportedFormats = "[\".vb\"]", ExportFormat = "ZIP" },
            new() { Name = "Nintendo Game & Watch", ShortName = "gw", RecalboxFolderName = "gw", SupportedFormats = "[\".gw\"]", ExportFormat = "ZIP" },
            new() { Name = "Sega Master System", ShortName = "mastersystem", RecalboxFolderName = "mastersystem", SupportedFormats = "[\".sms\"]", ExportFormat = "ZIP" },
            new() { Name = "Sega Mega Drive", ShortName = "megadrive", RecalboxFolderName = "megadrive", SupportedFormats = "[\".md\", \".gen\", \".smd\"]", ExportFormat = "ZIP" },
            new() { Name = "Sega Game Gear", ShortName = "gamegear", RecalboxFolderName = "gamegear", SupportedFormats = "[\".gg\"]", ExportFormat = "ZIP" },
            new() { Name = "Sega Mega Drive 32X", ShortName = "sega32x", RecalboxFolderName = "sega32x", SupportedFormats = "[\".32x\"]", ExportFormat = "ZIP" },
            new() { Name = "Sega Mega CD", ShortName = "segacd", RecalboxFolderName = "segacd", SupportedFormats = "[\".chd\", \".cue\", \".iso\"]", ExportFormat = "ZIP" },
            new() { Name = "Sega Dreamcast", ShortName = "dreamcast", RecalboxFolderName = "dreamcast", SupportedFormats = "[\".chd\", \".gdi\"]", ExportFormat = "ZIP" },
            new() { Name = "Sony PlayStation", ShortName = "psx", RecalboxFolderName = "psx", SupportedFormats = "[\".bin\", \".cue\", \".iso\", \".chd\"]", ExportFormat = "ZIP" },
            new() { Name = "Sony PlayStation Portable", ShortName = "psp", RecalboxFolderName = "psp", SupportedFormats = "[\".iso\", \".cso\"]", ExportFormat = "ZIP" },
            new() { Name = "Atari 2600", ShortName = "atari2600", RecalboxFolderName = "atari2600", SupportedFormats = "[\".a26\"]", ExportFormat = "ZIP" },
            new() { Name = "Atari 5200", ShortName = "atari5200", RecalboxFolderName = "atari5200", SupportedFormats = "[\".a52\"]", ExportFormat = "ZIP" },
            new() { Name = "Atari 7800", ShortName = "atari7800", RecalboxFolderName = "atari7800", SupportedFormats = "[\".a78\"]", ExportFormat = "ZIP" },
            new() { Name = "Atari Lynx", ShortName = "lynx", RecalboxFolderName = "lynx", SupportedFormats = "[\".lynx\"]", ExportFormat = "ZIP" },
            new() { Name = "Atari Jaguar", ShortName = "jaguar", RecalboxFolderName = "jaguar", SupportedFormats = "[\".jag\"]", ExportFormat = "ZIP" },
            new() { Name = "Atari ST", ShortName = "atarist", RecalboxFolderName = "atarist", SupportedFormats = "[\".st\"]", ExportFormat = "ZIP" },
            new() { Name = "NEC PC Engine", ShortName = "pcengine", RecalboxFolderName = "pcengine", SupportedFormats = "[\".pce\"]", ExportFormat = "ZIP" },
            new() { Name = "NEC PC Engine CD", ShortName = "pcenginecd", RecalboxFolderName = "pcenginecd", SupportedFormats = "[\".chd\", \".cue\"]", ExportFormat = "ZIP" },
            new() { Name = "NEC SuperGrafx", ShortName = "supergrafx", RecalboxFolderName = "supergrafx", SupportedFormats = "[\".sgx\"]", ExportFormat = "ZIP" },
            new() { Name = "SNK Neo Geo", ShortName = "neogeo", RecalboxFolderName = "neogeo", SupportedFormats = "[\".ng\"]", ExportFormat = "ZIP" },
            new() { Name = "SNK Neo Geo Pocket", ShortName = "ngp", RecalboxFolderName = "ngp", SupportedFormats = "[\".ngp\"]", ExportFormat = "ZIP" },
            new() { Name = "SNK Neo Geo Pocket Color", ShortName = "ngpc", RecalboxFolderName = "ngpc", SupportedFormats = "[\".ngc\"]", ExportFormat = "ZIP" },
            new() { Name = "Bandai WonderSwan", ShortName = "ws", RecalboxFolderName = "ws", SupportedFormats = "[\".ws\"]", ExportFormat = "ZIP" },
            new() { Name = "Bandai WonderSwan Color", ShortName = "wsc", RecalboxFolderName = "wsc", SupportedFormats = "[\".wsc\"]", ExportFormat = "ZIP" },
            new() { Name = "Commodore 64", ShortName = "c64", RecalboxFolderName = "c64", SupportedFormats = "[\".rom\", \".prg\"]", ExportFormat = "ZIP" },
            new() { Name = "Amiga 600", ShortName = "amiga600", RecalboxFolderName = "amiga600", SupportedFormats = "[\".adf\"]", ExportFormat = "ZIP" },
            new() { Name = "Amiga 1200", ShortName = "amiga1200", RecalboxFolderName = "amiga1200", SupportedFormats = "[\".adf\"]", ExportFormat = "ZIP" },
            new() { Name = "Amstrad CPC", ShortName = "amstradcpc", RecalboxFolderName = "amstradcpc", SupportedFormats = "[\".dsk\"]", ExportFormat = "ZIP" },
            new() { Name = "Apple II", ShortName = "apple2", RecalboxFolderName = "apple2", SupportedFormats = "[\".dsk\"]", ExportFormat = "ZIP" },
            new() { Name = "MAME", ShortName = "mame", RecalboxFolderName = "mame", SupportedFormats = "[\".zip\"]", ExportFormat = "ZIP" },
            new() { Name = "DOS", ShortName = "dos", RecalboxFolderName = "dos", SupportedFormats = "[\".exe\", \".com\"]", ExportFormat = "ZIP" },
            new() { Name = "Cave Story", ShortName = "cavestory", RecalboxFolderName = "cavestory", SupportedFormats = "[\".exe\"]", ExportFormat = "ZIP" }
        };

        _context.Consoles.AddRange(consoles);

        // Seed database sources
        var databaseSources = new List<DatabaseSource>
        {
            new() { Name = "NoIntro", Description = "NoIntro datfiles for verified ROMs", IsActive = true },
            new() { Name = "Redump", Description = "Redump datfiles for optical discs (CD/DVD)", IsActive = true },
            new() { Name = "GoodSet", Description = "GoodSet datfiles for complete collections", IsActive = true }
        };

        _context.DatabaseSources.AddRange(databaseSources);

        // Seed exclusion filters
        SeedExclusionFiltersAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedExclusionFiltersIfNeededAsync()
    {
        if (!await _context.ExclusionFilters.AnyAsync())
        {
            SeedExclusionFiltersAsync();
            await _context.SaveChangesAsync();
        }
    }

    private void SeedExclusionFiltersAsync()
    {
        var defaultFilters = new List<ExclusionFilter>
        {
            // Images
            new() { FilterType = "Extension", FilterValue = ".jpg", IsDefault = true, IsActive = true, Description = "Images JPEG" },
            new() { FilterType = "Extension", FilterValue = ".jpeg", IsDefault = true, IsActive = true, Description = "Images JPEG" },
            new() { FilterType = "Extension", FilterValue = ".png", IsDefault = true, IsActive = true, Description = "Images PNG" },
            new() { FilterType = "Extension", FilterValue = ".gif", IsDefault = true, IsActive = true, Description = "Images GIF" },
            new() { FilterType = "Extension", FilterValue = ".bmp", IsDefault = true, IsActive = true, Description = "Images BMP" },
            new() { FilterType = "Extension", FilterValue = ".tif", IsDefault = true, IsActive = true, Description = "Images TIFF" },
            new() { FilterType = "Extension", FilterValue = ".tiff", IsDefault = true, IsActive = true, Description = "Images TIFF" },
            new() { FilterType = "Extension", FilterValue = ".webp", IsDefault = true, IsActive = true, Description = "Images WebP" },
            
            // Textes
            new() { FilterType = "Extension", FilterValue = ".txt", IsDefault = true, IsActive = true, Description = "Fichiers texte" },
            new() { FilterType = "Extension", FilterValue = ".nfo", IsDefault = true, IsActive = true, Description = "Fichiers NFO" },
            new() { FilterType = "Extension", FilterValue = ".diz", IsDefault = true, IsActive = true, Description = "Fichiers DIZ" },
            new() { FilterType = "Extension", FilterValue = ".md", IsDefault = true, IsActive = true, Description = "Fichiers Markdown" },
            new() { FilterType = "Extension", FilterValue = ".rtf", IsDefault = true, IsActive = true, Description = "Fichiers RTF" },
            
            // Exécutables
            new() { FilterType = "Extension", FilterValue = ".exe", IsDefault = true, IsActive = true, Description = "Exécutables Windows" },
            new() { FilterType = "Extension", FilterValue = ".dll", IsDefault = true, IsActive = true, Description = "Bibliothèques Windows" },
            new() { FilterType = "Extension", FilterValue = ".so", IsDefault = true, IsActive = true, Description = "Bibliothèques Linux" },
            new() { FilterType = "Extension", FilterValue = ".dylib", IsDefault = true, IsActive = true, Description = "Bibliothèques macOS" },
            new() { FilterType = "Extension", FilterValue = ".bat", IsDefault = true, IsActive = true, Description = "Scripts batch Windows" },
            new() { FilterType = "Extension", FilterValue = ".sh", IsDefault = true, IsActive = true, Description = "Scripts shell" },
            new() { FilterType = "Extension", FilterValue = ".cmd", IsDefault = true, IsActive = true, Description = "Scripts CMD Windows" },
            
            // Documents
            new() { FilterType = "Extension", FilterValue = ".doc", IsDefault = true, IsActive = true, Description = "Documents Word" },
            new() { FilterType = "Extension", FilterValue = ".docx", IsDefault = true, IsActive = true, Description = "Documents Word" },
            new() { FilterType = "Extension", FilterValue = ".pdf", IsDefault = true, IsActive = true, Description = "Documents PDF" },
            new() { FilterType = "Extension", FilterValue = ".xls", IsDefault = true, IsActive = true, Description = "Feuilles Excel" },
            new() { FilterType = "Extension", FilterValue = ".xlsx", IsDefault = true, IsActive = true, Description = "Feuilles Excel" },
            new() { FilterType = "Extension", FilterValue = ".ppt", IsDefault = true, IsActive = true, Description = "Présentations PowerPoint" },
            new() { FilterType = "Extension", FilterValue = ".pptx", IsDefault = true, IsActive = true, Description = "Présentations PowerPoint" },
            
            // Markup/Config
            new() { FilterType = "Extension", FilterValue = ".html", IsDefault = true, IsActive = true, Description = "Pages HTML" },
            new() { FilterType = "Extension", FilterValue = ".htm", IsDefault = true, IsActive = true, Description = "Pages HTML" },
            new() { FilterType = "Extension", FilterValue = ".xml", IsDefault = true, IsActive = true, Description = "Fichiers XML" },
            new() { FilterType = "Extension", FilterValue = ".json", IsDefault = true, IsActive = true, Description = "Fichiers JSON" },
            new() { FilterType = "Extension", FilterValue = ".yaml", IsDefault = true, IsActive = true, Description = "Fichiers YAML" },
            new() { FilterType = "Extension", FilterValue = ".yml", IsDefault = true, IsActive = true, Description = "Fichiers YAML" },
            new() { FilterType = "Extension", FilterValue = ".css", IsDefault = true, IsActive = true, Description = "Feuilles de style CSS" }
        };

        _context.ExclusionFilters.AddRange(defaultFilters);
    }
}

