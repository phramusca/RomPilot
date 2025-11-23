namespace RomPilot.Core.Models;

/// <summary>
/// Stores calculated checksums/hashes for ROM files.
/// </summary>
public class Checksum
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to RomFile
    /// </summary>
    public int RomFileId { get; set; }
    
    /// <summary>
    /// Hash type: "MD5", "SHA1", "SHA256", "CRC32"
    /// </summary>
    public string HashType { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash value in hexadecimal format
    /// </summary>
    public string HashValue { get; set; } = string.Empty;
    
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public RomFile RomFile { get; set; } = null!;
}

