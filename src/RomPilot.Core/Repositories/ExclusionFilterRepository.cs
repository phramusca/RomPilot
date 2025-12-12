using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Database;
using RomPilot.Core.Models;

namespace RomPilot.Core.Repositories;

/// <summary>
/// Repository implementation for ExclusionFilter entities.
/// </summary>
public class ExclusionFilterRepository : IExclusionFilterRepository
{
    private readonly RomPilotDbContext _context;

    public ExclusionFilterRepository(RomPilotDbContext context)
    {
        _context = context;
    }

    public async Task<ExclusionFilter?> GetByIdAsync(int id)
    {
        return await _context.ExclusionFilters.FindAsync(id);
    }

    public async Task<IEnumerable<ExclusionFilter>> GetActiveFiltersAsync()
    {
        return await _context.ExclusionFilters
            .Where(f => f.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExclusionFilter>> GetDefaultFiltersAsync()
    {
        return await _context.ExclusionFilters
            .Where(f => f.IsDefault)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExclusionFilter>> GetCustomFiltersAsync()
    {
        return await _context.ExclusionFilters
            .Where(f => !f.IsDefault)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExclusionFilter>> GetByTypeAsync(string filterType)
    {
        return await _context.ExclusionFilters
            .Where(f => f.FilterType == filterType)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExclusionFilter>> GetAllAsync()
    {
        return await _context.ExclusionFilters.ToListAsync();
    }

    public async Task<ExclusionFilter> AddAsync(ExclusionFilter filter)
    {
        _context.ExclusionFilters.Add(filter);
        await _context.SaveChangesAsync();
        return filter;
    }

    public async Task UpdateAsync(ExclusionFilter filter)
    {
        filter.UpdatedAt = DateTime.UtcNow;
        _context.ExclusionFilters.Update(filter);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var filter = await _context.ExclusionFilters.FindAsync(id);
        if (filter != null)
        {
            _context.ExclusionFilters.Remove(filter);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string filterType, string filterValue)
    {
        return await _context.ExclusionFilters
            .AnyAsync(f => f.FilterType == filterType && f.FilterValue == filterValue);
    }
}

