using RomPilot.Core.Models;

namespace RomPilot.Core.Preferences;

/// <summary>
/// Interface for managing user preferences.
/// </summary>
public interface IUserPreferencesService
{
    Task<string?> GetPreferenceAsync(string key);
    Task SetPreferenceAsync(string key, string value);
    Task<List<string>> GetRegionPriorityAsync();
    Task SetRegionPriorityAsync(List<string> regions);
    Task<List<string>> GetLanguagePriorityAsync();
    Task SetLanguagePriorityAsync(List<string> languages);
    Task<bool> GetExcludeBadDumpsAsync();
    Task SetExcludeBadDumpsAsync(bool exclude);
}

