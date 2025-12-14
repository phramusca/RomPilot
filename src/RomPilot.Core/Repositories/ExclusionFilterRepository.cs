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

        System.Console.WriteLine($"[ExclusionFilterRepository] UpdateAsync called for filter ID={filter.Id}, IsActive={filter.IsActive}");

        // Ensure EF Core tracks the changes
        var entry = _context.Entry(filter);
        System.Console.WriteLine($"[ExclusionFilterRepository] Entry state before: {entry.State}");

        if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Detached)
        {
            _context.ExclusionFilters.Update(filter);
        }
        else
        {
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        System.Console.WriteLine($"[ExclusionFilterRepository] Entry state after: {entry.State}");
        await _context.SaveChangesAsync();
        System.Console.WriteLine($"[ExclusionFilterRepository] SaveChangesAsync completed");
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

    public async Task SetFilterActiveAsync(int filterId, bool isActive)
    {
        System.Console.WriteLine($"[ExclusionFilterRepository] SetFilterActiveAsync: filterId={filterId}, isActive={isActive}");

        var filter = await _context.ExclusionFilters.FindAsync(filterId);
        if (filter == null)
        {
            System.Console.WriteLine($"[ExclusionFilterRepository] ERROR: Filter {filterId} not found");
            throw new ArgumentException($"Filtre {filterId} introuvable");
        }

        System.Console.WriteLine($"[ExclusionFilterRepository] Filter before: Id={filter.Id}, FilterValue={filter.FilterValue}, IsActive={filter.IsActive}");
        filter.IsActive = isActive;
        filter.UpdatedAt = DateTime.UtcNow;
        System.Console.WriteLine($"[ExclusionFilterRepository] Filter after modification: IsActive={filter.IsActive}");

        await _context.SaveChangesAsync();
        System.Console.WriteLine($"[ExclusionFilterRepository] SaveChangesAsync completed");

        // Verify the change was saved
        var verifyFilter = await _context.ExclusionFilters.FindAsync(filterId);
        System.Console.WriteLine($"[ExclusionFilterRepository] Verification: IsActive={verifyFilter?.IsActive}");
    }
}

