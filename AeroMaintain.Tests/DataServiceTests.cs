using System.IO;
using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class DataServiceTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"AeroMaintain_Test_{Guid.NewGuid()}");
    private readonly DataService _service;

    public DataServiceTests()
    {
        _service = new DataService(_tempDir);
    }

    [Fact]
    public async Task LoadEquipment_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        var result = await _service.LoadEquipmentAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadTroubleshootingRules_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        var result = await _service.LoadTroubleshootingRulesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveAndLoadEquipment_RoundTrip_PreservesAllFields()
    {
        var original = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "Test Engine",
            SerialNumber = "ENG-001",
            Category = "Engine",
            LastMaintenanceDate = new DateTime(2026, 1, 15),
            MaintenanceIntervalDays = 60,
            Status = EquipmentStatus.Watch,
            RecentIssueCount = 3,
            Notes = "Test note with details"
        };

        await _service.SaveEquipmentAsync([original]);
        var loaded = await _service.LoadEquipmentAsync();

        var item = Assert.Single(loaded);
        Assert.Equal(original.Id, item.Id);
        Assert.Equal("Test Engine", item.Name);
        Assert.Equal("ENG-001", item.SerialNumber);
        Assert.Equal("Engine", item.Category);
        Assert.Equal(new DateTime(2026, 1, 15), item.LastMaintenanceDate);
        Assert.Equal(60, item.MaintenanceIntervalDays);
        Assert.Equal(EquipmentStatus.Watch, item.Status);
        Assert.Equal(3, item.RecentIssueCount);
        Assert.Equal("Test note with details", item.Notes);
    }

    [Fact]
    public async Task SaveEquipment_OverwritesPreviousData_OnSecondSave()
    {
        var first = new Equipment { Name = "First Unit", SerialNumber = "F-001", Status = EquipmentStatus.Healthy };
        var second = new Equipment { Name = "Second Unit", SerialNumber = "S-001", Status = EquipmentStatus.Critical };

        await _service.SaveEquipmentAsync([first]);
        await _service.SaveEquipmentAsync([second]);
        var loaded = await _service.LoadEquipmentAsync();

        var item = Assert.Single(loaded);
        Assert.Equal("Second Unit", item.Name);
    }

    [Fact]
    public async Task SaveAndLoadEquipment_PreservesMultipleItems()
    {
        var items = new List<Equipment>
        {
            new() { Name = "Unit A", SerialNumber = "A-001", Status = EquipmentStatus.Healthy },
            new() { Name = "Unit B", SerialNumber = "B-001", Status = EquipmentStatus.Watch },
            new() { Name = "Unit C", SerialNumber = "C-001", Status = EquipmentStatus.Critical }
        };

        await _service.SaveEquipmentAsync(items);
        var loaded = await _service.LoadEquipmentAsync();

        Assert.Equal(3, loaded.Count);
        Assert.Contains(loaded, e => e.SerialNumber == "A-001");
        Assert.Contains(loaded, e => e.SerialNumber == "B-001");
        Assert.Contains(loaded, e => e.SerialNumber == "C-001");
    }

    [Fact]
    public async Task JsonIgnoreFields_AreNotPersisted_HealthScoreNotRestored()
    {
        var original = new Equipment
        {
            Name = "Scored Unit",
            SerialNumber = "SC-001",
            Status = EquipmentStatus.Healthy,
            HealthScore = 95,
            HealthLabel = "Stable"
        };

        await _service.SaveEquipmentAsync([original]);
        var loaded = await _service.LoadEquipmentAsync();

        var item = Assert.Single(loaded);
        Assert.Equal(0, item.HealthScore);
        Assert.Equal("Stable", item.HealthLabel);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }
}
