using System.IO;
using System.Text.Json;
using AeroMaintain.Data;
using AeroMaintain.Models;
using Microsoft.EntityFrameworkCore;

namespace AeroMaintain.Services;

public class DataService
{
    private readonly string _dbPath;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DataService() : this(ResolveDbPath()) { }

    public DataService(string dbPath)
    {
        _dbPath = dbPath;
        using var ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    public async Task<List<Equipment>> LoadEquipmentAsync()
    {
        await using var ctx = CreateContext();
        var items = await ctx.Equipment.ToListAsync();
        if (items.Count == 0)
        {
            await SeedEquipmentFromJsonAsync();
            await using var ctx2 = CreateContext();
            items = await ctx2.Equipment.ToListAsync();
        }
        return items;
    }

    public async Task SaveEquipmentAsync(IEnumerable<Equipment> equipment)
    {
        var incoming = equipment.ToList();
        await using var ctx = CreateContext();
        var existing = await ctx.Equipment.ToListAsync();

        var toDelete = existing.Where(e => incoming.All(n => n.Id != e.Id)).ToList();
        ctx.Equipment.RemoveRange(toDelete);

        foreach (var item in incoming)
        {
            var tracked = existing.FirstOrDefault(e => e.Id == item.Id);
            if (tracked is null)
                ctx.Equipment.Add(item);
            else
                ctx.Entry(tracked).CurrentValues.SetValues(item);
        }

        await ctx.SaveChangesAsync();
    }

    public async Task<List<TroubleshootingRule>> LoadTroubleshootingRulesAsync()
    {
        await using var ctx = CreateContext();
        var rules = await ctx.TroubleshootingRules.ToListAsync();
        if (rules.Count == 0)
        {
            await SeedRulesFromJsonAsync();
            await using var ctx2 = CreateContext();
            rules = await ctx2.TroubleshootingRules.ToListAsync();
        }
        return rules;
    }

    private async Task SeedEquipmentFromJsonAsync()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "equipment.json");
        if (!File.Exists(jsonPath)) return;

        await using var stream = File.OpenRead(jsonPath);
        var items = await JsonSerializer.DeserializeAsync<List<Equipment>>(stream, JsonOptions);
        if (items is null) return;

        await using var ctx = CreateContext();
        ctx.Equipment.AddRange(items);
        await ctx.SaveChangesAsync();
    }

    private async Task SeedRulesFromJsonAsync()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "troubleshooting_rules.json");
        if (!File.Exists(jsonPath)) return;

        await using var stream = File.OpenRead(jsonPath);
        var rules = await JsonSerializer.DeserializeAsync<List<TroubleshootingRule>>(stream, JsonOptions);
        if (rules is null) return;

        await using var ctx = CreateContext();
        ctx.TroubleshootingRules.AddRange(rules);
        await ctx.SaveChangesAsync();
    }

    private AeroMaintainDbContext CreateContext() => new(_dbPath);

    private static string ResolveDbPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AeroMaintain");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "aeromaintain.db");
    }
}
