using System.IO;
using System.Text.Json;
using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class DataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _dataDirectory;
    private readonly string _equipmentPath;
    private readonly string _troubleshootingPath;

    public DataService() : this(Path.Combine(AppContext.BaseDirectory, "Data")) { }

    public DataService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        _equipmentPath = Path.Combine(_dataDirectory, "equipment.json");
        _troubleshootingPath = Path.Combine(_dataDirectory, "troubleshooting_rules.json");

        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    public async Task<List<Equipment>> LoadEquipmentAsync()
    {
        if (!File.Exists(_equipmentPath))
        {
            return new List<Equipment>();
        }

        await using var stream = File.OpenRead(_equipmentPath);
        var items = await JsonSerializer.DeserializeAsync<List<Equipment>>(stream, JsonOptions);
        return items ?? new List<Equipment>();
    }

    public async Task SaveEquipmentAsync(IEnumerable<Equipment> equipment)
    {
        await using var stream = File.Create(_equipmentPath);
        await JsonSerializer.SerializeAsync(stream, equipment, JsonOptions);
    }

    public async Task<List<TroubleshootingRule>> LoadTroubleshootingRulesAsync()
    {
        if (!File.Exists(_troubleshootingPath))
        {
            return new List<TroubleshootingRule>();
        }

        await using var stream = File.OpenRead(_troubleshootingPath);
        var rules = await JsonSerializer.DeserializeAsync<List<TroubleshootingRule>>(stream, JsonOptions);
        return rules ?? new List<TroubleshootingRule>();
    }
}
