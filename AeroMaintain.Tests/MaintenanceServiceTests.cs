using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class MaintenanceServiceTests
{
    private readonly MaintenanceService _service = new();

    [Fact]
    public void BuildTasks_CalculatesDueDateAndImmediatePriority()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            Name = "Hydraulic Pump",
            SerialNumber = "P-100",
            Category = "Pump",
            LastMaintenanceDate = new DateTime(2026, 5, 1),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Healthy
        };

        var task = Assert.Single(_service.BuildTasks([equipment], referenceDate));

        Assert.Equal(new DateTime(2026, 5, 31), task.DueDate);
        Assert.Equal(-14, task.DaysRemaining);
        Assert.True(task.IsOverdue);
        Assert.Equal("Immediate", task.PriorityLabel);
    }

    [Fact]
    public void ApplyFilters_ReturnsOnlyWatchItemsDueWithinSevenDays()
    {
        var tasks = new[]
        {
            CreateTask("Watch Soon", EquipmentStatus.Watch, 5),
            CreateTask("Healthy Soon", EquipmentStatus.Healthy, 5),
            CreateTask("Watch Later", EquipmentStatus.Watch, 20),
            CreateTask("Watch Overdue", EquipmentStatus.Watch, -2)
        };

        var result = _service.ApplyFilters(
            tasks,
            "Watch",
            "Due within 7 days",
            new DateTime(2026, 6, 14));

        var task = Assert.Single(result);
        Assert.Equal("Watch Soon", task.EquipmentName);
    }

    [Fact]
    public void BuildTasks_AssignsMonitorPriority_ForWatchStatusDueLater()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            Name = "Watch Engine",
            SerialNumber = "E-200",
            Category = "Engine",
            LastMaintenanceDate = referenceDate.AddDays(-5),
            MaintenanceIntervalDays = 25,
            Status = EquipmentStatus.Watch
        };

        var task = Assert.Single(_service.BuildTasks([equipment], referenceDate));

        Assert.Equal(20, task.DaysRemaining);
        Assert.False(task.IsOverdue);
        Assert.Equal("Monitor", task.PriorityLabel);
    }

    [Fact]
    public void BuildTasks_AssignsRoutinePriority_ForHealthyEquipmentDueLater()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            Name = "Healthy Pump",
            SerialNumber = "P-300",
            Category = "Pump",
            LastMaintenanceDate = referenceDate.AddDays(-5),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Healthy
        };

        var task = Assert.Single(_service.BuildTasks([equipment], referenceDate));

        Assert.Equal(25, task.DaysRemaining);
        Assert.False(task.IsOverdue);
        Assert.Equal("Routine", task.PriorityLabel);
    }

    [Fact]
    public void BuildTasks_AssignsImmediatePriority_ForCriticalStatusRegardlessOfDays()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            Name = "Critical Unit",
            SerialNumber = "C-001",
            Category = "Turbine",
            LastMaintenanceDate = referenceDate.AddDays(-5),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Critical
        };

        var task = Assert.Single(_service.BuildTasks([equipment], referenceDate));

        Assert.Equal("Immediate", task.PriorityLabel);
    }

    [Fact]
    public void ApplyFilters_ReturnsOverdueTasks_WhenOverdueFilterSelected()
    {
        var tasks = new[]
        {
            CreateTask("Overdue A", EquipmentStatus.Healthy, -5),
            CreateTask("Overdue B", EquipmentStatus.Watch, -1),
            CreateTask("Not Due Yet", EquipmentStatus.Healthy, 10),
            CreateTask("Due Today", EquipmentStatus.Healthy, 0)
        };

        var result = _service.ApplyFilters(tasks, "All", "Overdue", new DateTime(2026, 6, 14));

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.True(t.IsOverdue));
    }

    [Fact]
    public void ApplyFilters_ReturnsAllTasks_WhenBothFiltersAreAll()
    {
        var tasks = new[]
        {
            CreateTask("A", EquipmentStatus.Healthy, -5),
            CreateTask("B", EquipmentStatus.Watch, 5),
            CreateTask("C", EquipmentStatus.Critical, 25)
        };

        var result = _service.ApplyFilters(tasks, "All", "All", new DateTime(2026, 6, 14));

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ApplyFilters_ReturnsTasksDueWithin30Days_WhenDue30FilterSelected()
    {
        var tasks = new[]
        {
            CreateTask("Soon", EquipmentStatus.Healthy, 15),
            CreateTask("Later", EquipmentStatus.Healthy, 45),
            CreateTask("Overdue", EquipmentStatus.Healthy, -3)
        };

        var result = _service.ApplyFilters(tasks, "All", "Due within 30 days", new DateTime(2026, 6, 14));

        var task = Assert.Single(result);
        Assert.Equal("Soon", task.EquipmentName);
    }

    private static MaintenanceTask CreateTask(
        string name,
        EquipmentStatus status,
        int daysRemaining)
    {
        return new MaintenanceTask
        {
            EquipmentName = name,
            EquipmentStatus = status,
            DaysRemaining = daysRemaining,
            DueDate = new DateTime(2026, 6, 14).AddDays(daysRemaining)
        };
    }
}
