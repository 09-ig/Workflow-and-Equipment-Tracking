using System.IO;
using AeroMaintain.Models;
using AeroMaintain.Services;
using Microsoft.Data.Sqlite;

namespace AeroMaintain.Tests;

public class DataServiceTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"AeroMaintain_Test_{Guid.NewGuid()}");
    private readonly DataService _service;

    public DataServiceTests()
    {
        Directory.CreateDirectory(_tempDir);
        _service = new DataService(Path.Combine(_tempDir, "test.db"), seedFromJson: false);
    }

    [Fact]
    public async Task LoadEquipment_ReturnsEmptyList_WhenDatabaseIsEmpty()
    {
        var result = await _service.LoadEquipmentAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadTroubleshootingRules_ReturnsEmptyList_WhenDatabaseIsEmpty()
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
    public async Task SaveEquipment_RemovesDeletedItems_OnSync()
    {
        var first  = new Equipment { Name = "First",  SerialNumber = "F-001", Status = EquipmentStatus.Healthy };
        var second = new Equipment { Name = "Second", SerialNumber = "S-001", Status = EquipmentStatus.Critical };

        await _service.SaveEquipmentAsync([first, second]);
        await _service.SaveEquipmentAsync([second]);   // remove first

        var loaded = await _service.LoadEquipmentAsync();
        var item = Assert.Single(loaded);
        Assert.Equal("Second", item.Name);
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
    public async Task ComputedFields_AreNotPersisted_HealthScoreResetOnLoad()
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
        Assert.Equal(0, item.HealthScore);       // not persisted
        Assert.Equal("Stable", item.HealthLabel); // default value from model
    }

    [Fact]
    public async Task SaveEquipment_WritesAuditEntries_ForCreateUpdateAndDelete()
    {
        var equipment = new Equipment
        {
            Name = "Audited Pump",
            SerialNumber = "AUD-001",
            Category = "Pump",
            Status = EquipmentStatus.Healthy
        };

        await _service.SaveEquipmentAsync([equipment], "tester", UserRole.Supervisor);

        equipment.Status = EquipmentStatus.Critical;
        await _service.SaveEquipmentAsync([equipment], "tester", UserRole.Supervisor);

        await _service.SaveEquipmentAsync([], "tester", UserRole.Supervisor);

        var auditLogs = await _service.LoadAuditLogsAsync();

        Assert.Contains(auditLogs, a =>
            a.Action == "Created" &&
            a.EntityName == nameof(Equipment) &&
            a.EntityId == equipment.Id &&
            a.UserName == "tester" &&
            a.UserRole == nameof(UserRole.Supervisor));

        Assert.Contains(auditLogs, a =>
            a.Action == "Updated" &&
            a.FieldName == nameof(Equipment.Status) &&
            a.OldValue == "Healthy" &&
            a.NewValue == "Critical");

        Assert.Contains(auditLogs, a =>
            a.Action == "Deleted" &&
            a.EntityName == nameof(Equipment) &&
            a.EntityId == equipment.Id);
    }

    [Fact]
    public async Task RecordMaintenance_CreatesHistoryEntryAndUpdatesEquipmentDate()
    {
        var equipment = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = "History Engine",
            SerialNumber = "HIS-001",
            Category = "Engine",
            LastMaintenanceDate = new DateTime(2026, 1, 1),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Watch
        };

        await _service.SaveEquipmentAsync([equipment], "setup");

        var log = await _service.RecordMaintenanceAsync(
            equipment.Id,
            "tech.one",
            "Compressor inspection completed.",
            "Filter kit",
            125.50m,
            2.5,
            new DateTime(2026, 2, 15),
            "No abnormal vibration found.",
            "tech.one",
            UserRole.Technician);

        var loadedEquipment = await _service.LoadEquipmentAsync();
        var updated = Assert.Single(loadedEquipment);
        Assert.Equal(new DateTime(2026, 2, 15), updated.LastMaintenanceDate);

        var history = await _service.LoadMaintenanceLogsAsync(equipment.Id);
        var historyEntry = Assert.Single(history);
        Assert.Equal(log.Id, historyEntry.Id);
        Assert.Equal("tech.one", historyEntry.PerformedBy);
        Assert.Equal("Compressor inspection completed.", historyEntry.WorkSummary);
        Assert.Equal("Filter kit", historyEntry.PartsReplaced);
        Assert.Equal(125.50m, historyEntry.Cost);
        Assert.Equal(2.5, historyEntry.LaborHours);

        var auditLogs = await _service.LoadAuditLogsAsync();
        Assert.Contains(auditLogs, a =>
            a.EntityName == nameof(MaintenanceLog) &&
            a.EntityId == log.Id &&
            a.Action == "Created" &&
            a.UserRole == nameof(UserRole.Technician));
        Assert.Contains(auditLogs, a =>
            a.EntityName == nameof(Equipment) &&
            a.EntityId == equipment.Id &&
            a.FieldName == nameof(Equipment.LastMaintenanceDate) &&
            a.OldValue == "2026-01-01" &&
            a.NewValue == "2026-02-15");
    }

    [Fact]
    public async Task SaveTroubleshootingRules_WritesAuditEntries_ForCreateUpdateAndDelete()
    {
        var rule = new TroubleshootingRule
        {
            Id = Guid.NewGuid(),
            Symptom = "Hydraulic drift",
            PossibleCauses = ["Internal leakage"],
            RecommendedChecks = ["Inspect actuator seal"]
        };

        await _service.SaveTroubleshootingRulesAsync([rule], "supervisor", UserRole.Supervisor);

        rule.RecommendedChecks = ["Inspect actuator seal", "Check valve response"];
        await _service.SaveTroubleshootingRulesAsync([rule], "supervisor", UserRole.Supervisor);

        await _service.SaveTroubleshootingRulesAsync([], "supervisor", UserRole.Supervisor);

        var auditLogs = await _service.LoadAuditLogsAsync();
        Assert.Contains(auditLogs, audit =>
            audit.EntityName == nameof(TroubleshootingRule) &&
            audit.EntityId == rule.Id &&
            audit.Action == "Created" &&
            audit.UserRole == nameof(UserRole.Supervisor));
        Assert.Contains(auditLogs, audit =>
            audit.EntityName == nameof(TroubleshootingRule) &&
            audit.FieldName == nameof(TroubleshootingRule.RecommendedChecks) &&
            audit.NewValue == "Inspect actuator seal; Check valve response");
        Assert.Contains(auditLogs, audit =>
            audit.EntityName == nameof(TroubleshootingRule) &&
            audit.EntityId == rule.Id &&
            audit.Action == "Deleted");
    }

    [Fact]
    public async Task Constructor_BaselinesLegacyDatabaseAndAppliesProductionLoggingMigration()
    {
        var legacyDbPath = Path.Combine(_tempDir, "legacy.db");
        var legacyEquipmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        CreateLegacyDatabase(legacyDbPath, legacyEquipmentId);

        var service = new DataService(legacyDbPath, seedFromJson: false);
        var equipment = Assert.Single(await service.LoadEquipmentAsync());

        Assert.Equal(legacyEquipmentId, equipment.Id);
        Assert.Equal("Legacy Pump", equipment.Name);

        var log = await service.RecordMaintenanceAsync(
            legacyEquipmentId,
            "legacy.tech",
            "Legacy database upgrade check.",
            "None",
            0,
            1,
            new DateTime(2026, 3, 1),
            string.Empty,
            "legacy.tech");

        var history = await service.LoadMaintenanceLogsAsync(legacyEquipmentId);
        Assert.Single(history);

        var auditLogs = await service.LoadAuditLogsAsync();
        Assert.Contains(auditLogs, a =>
            a.EntityName == nameof(MaintenanceLog) &&
            a.EntityId == log.Id &&
            a.Action == "Created");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static void CreateLegacyDatabase(string dbPath, Guid equipmentId)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Pooling = false
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            CREATE TABLE "Equipment" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Equipment" PRIMARY KEY,
                "Name" TEXT NOT NULL,
                "SerialNumber" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "LastMaintenanceDate" TEXT NOT NULL,
                "MaintenanceIntervalDays" INTEGER NOT NULL,
                "Status" TEXT NOT NULL,
                "RecentIssueCount" INTEGER NOT NULL,
                "Notes" TEXT NOT NULL
            );

            CREATE TABLE "TroubleshootingRules" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_TroubleshootingRules" PRIMARY KEY,
                "Symptom" TEXT NOT NULL,
                "PossibleCauses" TEXT NOT NULL,
                "RecommendedChecks" TEXT NOT NULL
            );

            INSERT INTO "Equipment" (
                "Id",
                "Name",
                "SerialNumber",
                "Category",
                "LastMaintenanceDate",
                "MaintenanceIntervalDays",
                "Status",
                "RecentIssueCount",
                "Notes")
            VALUES (
                '{equipmentId}',
                'Legacy Pump',
                'LEG-001',
                'Pump',
                '2026-01-01 00:00:00',
                30,
                'Healthy',
                0,
                '');
            """;
        command.ExecuteNonQuery();
    }
}
