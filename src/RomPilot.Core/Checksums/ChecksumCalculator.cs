using System.Security.Cryptography;
using System.Text;

namespace RomPilot.Core.Checksums;

/// <summary>
/// Implementation of checksum calculator for ROM files.
/// Supports MD5, SHA1, SHA256, and CRC32.
/// </summary>
public class ChecksumCalculator : IChecksumCalculator
{
    public async Task<string> CalculateMD5Async(Stream stream)
    {
        stream.Position = 0;
        using var md5 = MD5.Create();
        var hashBytes = await md5.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<string> CalculateSHA1Async(Stream stream)
    {
        stream.Position = 0;
        using var sha1 = SHA1.Create();
        var hashBytes = await sha1.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<string> CalculateSHA256Async(Stream stream)
    {
        stream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<string> CalculateCRC32Async(Stream stream)
    {
        stream.Position = 0;
        uint crc = 0xFFFFFFFF;
        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                crc = Crc32Table[(crc ^ buffer[i]) & 0xFF] ^ (crc >> 8);
            }
        }

        crc ^= 0xFFFFFFFF;
        return crc.ToString("X8").ToLowerInvariant();
    }

    public async Task<Dictionary<string, string>> CalculateAllAsync(Stream stream)
    {
        var results = new Dictionary<string, string>();
        
        // Calculate all checksums in parallel for better performance
        var tasks = new[]
        {
            Task.Run(async () => ("MD5", await CalculateMD5Async(stream))),
            Task.Run(async () => ("SHA1", await CalculateSHA1Async(stream))),
            Task.Run(async () => ("SHA256", await CalculateSHA256Async(stream))),
            Task.Run(async () => ("CRC32", await CalculateCRC32Async(stream)))
        };

        await Task.WhenAll(tasks);
        
        foreach (var task in tasks)
        {
            var (hashType, hashValue) = await task;
            results[hashType] = hashValue;
        }

        return results;
    }

    // CRC32 lookup table
    private static readonly uint[] Crc32Table = GenerateCrc32Table();

    private static uint[] GenerateCrc32Table()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
            table[i] = crc;
        }
        return table;
    }
}

