using System.IO;
using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class ExportServiceTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"AeroMaintain_Export_{Guid.NewGuid()}");
    private readonly ExportService _service = new();

    private static Equipment MakeEquipment(string name = "Test Engine", string serial = "ENG-001") =>
        new()
        {
            Name = name,
            SerialNumber = serial,
            Category = "Engine",
            Status = EquipmentStatus.Healthy,
            LastMaintenanceDate = new DateTime(2026, 5, 1),
            MaintenanceIntervalDays = 30,
            RecentIssueCount = 0,
            HealthScore = 100,
            HealthLabel = "Stable"
        };

    private static MaintenanceTask MakeTask(string name = "Test Engine", int daysRemaining = 10) =>
        new()
        {
            EquipmentName = name,
            SerialNumber = "ENG-001",
            Category = "Engine",
            DueDate = new DateTime(2026, 7, 1),
            DaysRemaining = daysRemaining,
            EquipmentStatus = EquipmentStatus.Healthy,
            PriorityLabel = "Routine"
        };

    [Fact]
    public void ExportEquipmentCsv_CreatesFile_InSpecifiedDirectory()
    {
        var path = _service.ExportEquipmentCsv([MakeEquipment()], _tempDir);

        Assert.True(File.Exists(path));
        Assert.StartsWith(_tempDir, path);
    }

    [Fact]
    public void ExportEquipmentCsv_FileContainsHeaderRow()
    {
        var path = _service.ExportEquipmentCsv([MakeEquipment()], _tempDir);

        var lines = File.ReadAllLines(path);
        Assert.Contains("Name", lines[0]);
        Assert.Contains("SerialNumber", lines[0]);
        Assert.Contains("HealthScore", lines[0]);
    }

    [Fact]
    public void ExportEquipmentCsv_FileContainsEquipmentData()
    {
        var path = _service.ExportEquipmentCsv([MakeEquipment("Hydraulic Pump", "PMP-001")], _tempDir);

        var content = File.ReadAllText(path);
        Assert.Contains("Hydraulic Pump", content);
        Assert.Contains("PMP-001", content);
    }

    [Fact]
    public void ExportEquipmentCsv_WritesOneDataRowPerEquipment()
    {
        var equipment = new[] { MakeEquipment("A", "A-001"), MakeEquipment("B", "B-001"), MakeEquipment("C", "C-001") };

        var path = _service.ExportEquipmentCsv(equipment, _tempDir);

        var lines = File.ReadAllLines(path);
        Assert.Equal(4, lines.Length); // 1 header + 3 data rows
    }

    [Fact]
    public void ExportEquipmentCsv_EscapesCommasInNames()
    {
        var item = MakeEquipment("Engine, Stage 2", "ENG-002");

        var path = _service.ExportEquipmentCsv([item], _tempDir);

        var content = File.ReadAllText(path);
        Assert.Contains("\"Engine, Stage 2\"", content);
    }

    [Fact]
    public void ExportTasksCsv_CreatesFile_InSpecifiedDirectory()
    {
        var path = _service.ExportTasksCsv([MakeTask()], _tempDir);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void ExportTasksCsv_FileContainsHeaderRow()
    {
        var path = _service.ExportTasksCsv([MakeTask()], _tempDir);

        var lines = File.ReadAllLines(path);
        Assert.Contains("EquipmentName", lines[0]);
        Assert.Contains("DaysRemaining", lines[0]);
        Assert.Contains("Priority", lines[0]);
    }

    [Fact]
    public void ExportTasksCsv_FileContainsTaskData()
    {
        var path = _service.ExportTasksCsv([MakeTask("Turbine Unit", 5)], _tempDir);

        var content = File.ReadAllText(path);
        Assert.Contains("Turbine Unit", content);
        Assert.Contains("5", content);
    }

    [Fact]
    public void BuildTextReport_CreatesFile_WithSummaryHeader()
    {
        var path = _service.BuildTextReport([MakeEquipment()], [MakeTask()], 100.0, _tempDir);

        Assert.True(File.Exists(path));
        var content = File.ReadAllText(path);
        Assert.Contains("AeroMaintain", content);
    }

    [Fact]
    public void BuildTextReport_IncludesCorrectEquipmentCount()
    {
        var equipment = new[] { MakeEquipment("A", "A-001"), MakeEquipment("B", "B-001") };

        var path = _service.BuildTextReport(equipment, [], 95.0, _tempDir);

        var content = File.ReadAllText(path);
        Assert.Contains("Total equipment: 2", content);
    }

    [Fact]
    public void BuildTextReport_ShowsNoImmediatePriorityItems_WhenAllTasksAreRoutine()
    {
        var path = _service.BuildTextReport([MakeEquipment()], [MakeTask(daysRemaining: 20)], 100.0, _tempDir);

        var content = File.ReadAllText(path);
        Assert.Contains("No immediate priority items", content);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }
}
