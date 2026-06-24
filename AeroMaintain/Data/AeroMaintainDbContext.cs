using System.Text.Json;
using AeroMaintain.Models;
using Microsoft.EntityFrameworkCore;

namespace AeroMaintain.Data;

public class AeroMaintainDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<TroubleshootingRule> TroubleshootingRules { get; set; } = null!;

    public AeroMaintainDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions();

        // Store status enum as its string name for readable DB rows
        modelBuilder.Entity<Equipment>()
            .Property(e => e.Status)
            .HasConversion<string>();

        // Computed/runtime-only fields — not persisted
        modelBuilder.Entity<Equipment>()
            .Ignore(e => e.NextDueDate)
            .Ignore(e => e.DaysToDue)
            .Ignore(e => e.IsOverdue)
            .Ignore(e => e.HealthScore)
            .Ignore(e => e.HealthLabel);

        // Store List<string> as JSON text columns (SQLite has no array type)
        modelBuilder.Entity<TroubleshootingRule>()
            .Property(r => r.PossibleCauses)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>());

        modelBuilder.Entity<TroubleshootingRule>()
            .Property(r => r.RecommendedChecks)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>());
    }
}
