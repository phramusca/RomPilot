using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for ExportConfiguration entities.
/// </summary>
public class ExportConfigurationRepository : IExportConfigurationRepository
{
    private readonly RomPilotDbContext _context;

    public ExportConfigurationRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<ExportConfiguration?> GetByIdAsync(int id)
    {
        return await _context.ExportConfigurations.FindAsync(id);
    }

    public async Task<ExportConfiguration?> GetByNameAsync(string name)
    {
        return await _context.ExportConfigurations
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<IEnumerable<ExportConfiguration>> GetByPlatformAsync(string platform)
    {
        return await _context.ExportConfigurations
            .Where(c => c.Platform == platform)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExportConfiguration>> GetByExportTypeAsync(string exportType)
    {
        return await _context.ExportConfigurations
            .Where(c => c.ExportType == exportType)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExportConfiguration>> GetActiveConfigurationsAsync()
    {
        return await _context.ExportConfigurations
            .Where(c => c.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExportConfiguration>> GetAllAsync()
    {
        return await _context.ExportConfigurations.ToListAsync();
    }

    public async Task<ExportConfiguration> AddAsync(ExportConfiguration configuration)
    {
        _context.ExportConfigurations.Add(configuration);
        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task UpdateAsync(ExportConfiguration configuration)
    {
        configuration.UpdatedAt = DateTime.UtcNow;
        _context.ExportConfigurations.Update(configuration);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var configuration = await _context.ExportConfigurations.FindAsync(id);
        if (configuration != null)
        {
            _context.ExportConfigurations.Remove(configuration);
            await _context.SaveChangesAsync();
        }
    }
}

