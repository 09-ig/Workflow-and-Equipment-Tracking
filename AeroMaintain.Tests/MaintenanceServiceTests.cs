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
