using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RomPilot.Core.Database;

/// <summary>
/// Factory for creating DbContext instances at design time (for migrations).
/// </summary>
public class RomPilotDbContextFactory : IDesignTimeDbContextFactory<RomPilotDbContext>
{
    public RomPilotDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RomPilotDbContext>();
        var connectionString = "Data Source=rompilot.db";
        optionsBuilder.UseSqlite(connectionString);

        return new RomPilotDbContext(optionsBuilder.Options);
    }
}

