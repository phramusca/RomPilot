namespace RomPilot.Core.Checksums;

/// <summary>
/// Interface for calculating checksums/hashes for ROM files.
/// </summary>
public interface IChecksumCalculator
{
    /// <summary>
    /// Calculates MD5 hash for a file stream.
    /// </summary>
    Task<string> CalculateMD5Async(Stream stream);

    /// <summary>
    /// Calculates SHA1 hash for a file stream.
    /// </summary>
    Task<string> CalculateSHA1Async(Stream stream);

    /// <summary>
    /// Calculates SHA256 hash for a file stream.
    /// </summary>
    Task<string> CalculateSHA256Async(Stream stream);

    /// <summary>
    /// Calculates CRC32 hash for a file stream.
    /// </summary>
    Task<string> CalculateCRC32Async(Stream stream);

    /// <summary>
    /// Calculates all required checksums for a file stream.
    /// </summary>
    Task<Dictionary<string, string>> CalculateAllAsync(Stream stream);
}

