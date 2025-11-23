using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Preferences;

/// <summary>
/// Implementation of user preferences service.
/// </summary>
public class UserPreferencesService : IUserPreferencesService
{
    private readonly RomPilotDbContext _context;

    private const string RegionPriorityKey = "region_priority";
    private const string LanguagePriorityKey = "language_priority";
    private const string ExcludeBadDumpsKey = "exclude_bad_dumps";

    public UserPreferencesService(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetPreferenceAsync(string key)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.Key == key);
        return preference?.Value;
    }

    public async Task SetPreferenceAsync(string key, string value)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.Key == key);

        if (preference == null)
        {
            preference = new UserPreferences
            {
                Key = key,
                Value = value,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserPreferences.Add(preference);
        }
        else
        {
            preference.Value = value;
            preference.UpdatedAt = DateTime.UtcNow;
            _context.UserPreferences.Update(preference);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetRegionPriorityAsync()
    {
        var value = await GetPreferenceAsync(RegionPriorityKey);
        if (string.IsNullOrEmpty(value))
        {
            return new List<string> { "EUR", "USA", "JAP" }; // Default
        }
        return JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();
    }

    public async Task SetRegionPriorityAsync(List<string> regions)
    {
        var json = JsonSerializer.Serialize(regions);
        await SetPreferenceAsync(RegionPriorityKey, json);
    }

    public async Task<List<string>> GetLanguagePriorityAsync()
    {
        var value = await GetPreferenceAsync(LanguagePriorityKey);
        if (string.IsNullOrEmpty(value))
        {
            return new List<string> { "FR", "EN" }; // Default
        }
        return JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>();
    }

    public async Task SetLanguagePriorityAsync(List<string> languages)
    {
        var json = JsonSerializer.Serialize(languages);
        await SetPreferenceAsync(LanguagePriorityKey, json);
    }

    public async Task<bool> GetExcludeBadDumpsAsync()
    {
        var value = await GetPreferenceAsync(ExcludeBadDumpsKey);
        return value == "true";
    }

    public async Task SetExcludeBadDumpsAsync(bool exclude)
    {
        await SetPreferenceAsync(ExcludeBadDumpsKey, exclude ? "true" : "false");
    }
}

