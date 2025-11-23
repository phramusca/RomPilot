namespace RomPilot.Core.Models;

/// <summary>
/// User preferences for filtering and selection.
/// </summary>
public class UserPreferences
{
    public int Id { get; set; }
    
    /// <summary>
    /// Preference key (e.g., "region_priority", "language_priority")
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Preference value (JSON if complex)
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

