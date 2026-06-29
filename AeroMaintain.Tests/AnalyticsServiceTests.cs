using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class AnalyticsServiceTests
{
    private readonly AnalyticsService _service = new();

    [Fact]
    public void BuildSnapshot_CalculatesMtbfMttrAndCostTotals()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = new Equipment
        {
            Id = equipmentId,
            Name = "Hydraulic Pump",
            SerialNumber = "P-100",
            Category = "Pump",
            LastMaintenanceDate = new DateTime(2026, 3, 2),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Healthy
        };
        var logs = new[]
        {
            CreateLog(equipmentId, "Hydraulic Pump", new DateTime(2026, 1, 1), 2, 100),
            CreateLog(equipmentId, "Hydraulic Pump", new DateTime(2026, 1, 31), 4, 200),
            CreateLog(equipmentId, "Hydraulic Pump", new DateTime(2026, 3, 2), 6, 300)
        };
        var tasks = new[]
        {
            new MaintenanceTask
            {
                EquipmentId = equipmentId,
                EquipmentName = "Hydraulic Pump",
                DaysRemaining = 10,
                DueDate = new DateTime(2026, 4, 1),
                EquipmentStatus = EquipmentStatus.Healthy
            }
        };

        var snapshot = _service.BuildSnapshot([equipment], logs, tasks, new DateTime(2026, 3, 22));

        Assert.Equal(3, snapshot.MaintenanceEventCount);
        Assert.Equal(30, snapshot.FleetAverageDaysBetweenMaintenance, 1);
        Assert.Equal(4, snapshot.AverageRepairHours, 1);
        Assert.Equal(600, snapshot.TotalMaintenanceCost);
        Assert.Equal(200, snapshot.AverageCostPerEvent);

        var summary = Assert.Single(snapshot.EquipmentSummaries);
        Assert.Equal(30, summary.AverageDaysBetweenMaintenance, 1);
        Assert.Equal(4, summary.AverageRepairHours, 1);
        Assert.Equal("Normal", summary.ReliabilityLabel);
        Assert.Equal(new DateTime(2026, 3, 2), summary.LastMaintenanceCompletedOn);
    }

    [Fact]
    public void BuildSnapshot_GroupsCategoriesAndFlagsImmediateRisk()
    {
        var criticalId = Guid.NewGuid();
        var overdueId = Guid.NewGuid();
        var equipment = new[]
        {
            new Equipment
            {
                Id = criticalId,
                Name = "Main Engine",
                SerialNumber = "E-100",
                Category = "Engine",
                Status = EquipmentStatus.Critical,
                LastMaintenanceDate = new DateTime(2026, 5, 1),
                MaintenanceIntervalDays = 30
            },
            new Equipment
            {
                Id = overdueId,
                Name = "Aux Engine",
                SerialNumber = "E-200",
                Category = "Engine",
                Status = EquipmentStatus.Healthy,
                LastMaintenanceDate = new DateTime(2026, 4, 1),
                MaintenanceIntervalDays = 30
            }
        };
        var logs = new[]
        {
            CreateLog(criticalId, "Main Engine", new DateTime(2026, 4, 1), 1, 100),
            CreateLog(criticalId, "Main Engine", new DateTime(2026, 5, 1), 3, 300)
        };
        var tasks = new[]
        {
            new MaintenanceTask { EquipmentId = criticalId, DaysRemaining = 5, EquipmentStatus = EquipmentStatus.Critical },
            new MaintenanceTask { EquipmentId = overdueId, DaysRemaining = -2, EquipmentStatus = EquipmentStatus.Healthy }
        };

        var snapshot = _service.BuildSnapshot(equipment, logs, tasks, new DateTime(2026, 6, 1));

        Assert.Equal(1, snapshot.CriticalCount);
        Assert.Equal(1, snapshot.OverdueCount);

        var category = Assert.Single(snapshot.CategorySummaries);
        Assert.Equal("Engine", category.Category);
        Assert.Equal(2, category.EquipmentCount);
        Assert.Equal(2, category.MaintenanceEventCount);
        Assert.Equal(400, category.TotalCost);
        Assert.Equal(200, category.AverageCostPerEvent);
        Assert.Equal(2, category.AverageRepairHours, 1);
        Assert.Equal(1, category.CriticalCount);
        Assert.Equal(1, category.OverdueCount);
        Assert.Equal("Immediate", category.RiskLabel);
    }

    [Fact]
    public void BuildSnapshot_HandlesEmptyFleet()
    {
        var snapshot = _service.BuildSnapshot([], [], [], new DateTime(2026, 6, 1));

        Assert.Equal(0, snapshot.EquipmentCount);
        Assert.Equal(0, snapshot.MaintenanceEventCount);
        Assert.Equal(0, snapshot.FleetAverageDaysBetweenMaintenance);
        Assert.Equal(0, snapshot.AverageRepairHours);
        Assert.Empty(snapshot.EquipmentSummaries);
        Assert.Empty(snapshot.CategorySummaries);
        Assert.Contains("Add equipment", snapshot.ReliabilityNote);
    }

    private static MaintenanceLog CreateLog(
        Guid equipmentId,
        string equipmentName,
        DateTime completedOn,
        double laborHours,
        decimal cost)
    {
        return new MaintenanceLog
        {
            EquipmentId = equipmentId,
            EquipmentName = equipmentName,
            CompletedOn = completedOn,
            LaborHours = laborHours,
            Cost = cost,
            PerformedBy = "Tester",
            WorkSummary = "Completed"
        };
    }
}
