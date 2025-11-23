using Microsoft.EntityFrameworkCore;
using RomPilot.Core.Models;

namespace RomPilot.Core.Database;

/// <summary>
/// Entity Framework Core database context for RomPilot application.
/// </summary>
public class RomPilotDbContext : DbContext
{
    public RomPilotDbContext(DbContextOptions<RomPilotDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.Console> Consoles { get; set; } = null!;
    public DbSet<RomFile> RomFiles { get; set; } = null!;
    public DbSet<Checksum> Checksums { get; set; } = null!;
    public DbSet<DatabaseSource> DatabaseSources { get; set; } = null!;
    public DbSet<GameEntry> GameEntries { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<GameRomVersion> GameRomVersions { get; set; } = null!;
    public DbSet<Metadata> Metadata { get; set; } = null!;
    public DbSet<UserData> UserData { get; set; } = null!;
    public DbSet<UserPreferences> UserPreferences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Console configuration
        modelBuilder.Entity<Models.Console>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.ShortName).IsUnique();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.ShortName).IsRequired();
            entity.Property(e => e.RecalboxFolderName).IsRequired();
        });

        // RomFile configuration
        modelBuilder.Entity<RomFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConsoleId);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileName).IsRequired();
            entity.HasOne(e => e.Console)
                .WithMany(c => c.RomFiles)
                .HasForeignKey(e => e.ConsoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Checksum configuration
        modelBuilder.Entity<Checksum>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.HashType, e.HashValue });
            entity.HasIndex(e => e.RomFileId);
            entity.Property(e => e.HashType).IsRequired();
            entity.Property(e => e.HashValue).IsRequired();
            entity.HasOne(e => e.RomFile)
                .WithMany(r => r.Checksums)
                .HasForeignKey(e => e.RomFileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasAlternateKey(e => new { e.RomFileId, e.HashType });
        });

        // DatabaseSource configuration
        modelBuilder.Entity<DatabaseSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
        });

        // GameEntry configuration
        modelBuilder.Entity<GameEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.HashType, e.HashValue });
            entity.HasIndex(e => e.DatabaseSourceId);
            entity.HasIndex(e => e.ConsoleId);
            entity.Property(e => e.GameName).IsRequired();
            entity.Property(e => e.HashType).IsRequired();
            entity.Property(e => e.HashValue).IsRequired();
            entity.HasOne(e => e.DatabaseSource)
                .WithMany(d => d.GameEntries)
                .HasForeignKey(e => e.DatabaseSourceId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Console)
                .WithMany(c => c.GameEntries)
                .HasForeignKey(e => e.ConsoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Game configuration
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConsoleId);
            entity.Property(e => e.Name).IsRequired();
            entity.HasOne(e => e.Console)
                .WithMany(c => c.Games)
                .HasForeignKey(e => e.ConsoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SelectedDatabaseSource)
                .WithMany()
                .HasForeignKey(e => e.SelectedDatabaseSourceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.SelectedRomFile)
                .WithMany()
                .HasForeignKey(e => e.SelectedRomFileId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // GameRomVersion configuration
        modelBuilder.Entity<GameRomVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GameId);
            entity.HasIndex(e => e.RomFileId);
            entity.HasOne(e => e.Game)
                .WithMany(g => g.GameRomVersions)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.RomFile)
                .WithMany(r => r.GameRomVersions)
                .HasForeignKey(e => e.RomFileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.GameEntry)
                .WithMany(ge => ge.GameRomVersions)
                .HasForeignKey(e => e.GameEntryId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasAlternateKey(e => new { e.GameId, e.RomFileId });
        });

        // Metadata configuration
        modelBuilder.Entity<Metadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GameId);
            entity.Property(e => e.Source).IsRequired();
            entity.HasOne(e => e.Game)
                .WithMany(g => g.Metadata)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserData configuration
        modelBuilder.Entity<UserData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GameId).IsUnique();
            entity.HasIndex(e => e.IsFavorite);
            entity.HasOne(e => e.Game)
                .WithOne(g => g.UserData)
                .HasForeignKey<UserData>(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserPreferences configuration
        modelBuilder.Entity<UserPreferences>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Value).IsRequired();
        });
    }
}

