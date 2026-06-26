using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class NotificationServiceTests
{
    private readonly NotificationService _service = new();

    [Fact]
    public void BuildAlerts_ReturnsCriticalAlert_ForOverdueEquipment()
    {
        var task = CreateTask("Overdue Pump", EquipmentStatus.Healthy, -3);

        var alert = Assert.Single(_service.BuildAlerts([task]));

        Assert.Equal(MaintenanceAlertSeverity.Critical, alert.Severity);
        Assert.Equal("Overdue Pump is overdue by 3 days.", alert.Message);
        Assert.Equal("Assign immediate maintenance", alert.ActionLabel);
    }

    [Fact]
    public void BuildAlerts_ReturnsCriticalAlert_ForCriticalEquipmentEvenWhenNotOverdue()
    {
        var task = CreateTask("Critical Turbine", EquipmentStatus.Critical, 12);

        var alert = Assert.Single(_service.BuildAlerts([task]));

        Assert.Equal(MaintenanceAlertSeverity.Critical, alert.Severity);
        Assert.Equal("Critical Turbine is marked Critical and due in 12 days.", alert.Message);
    }

    [Fact]
    public void BuildAlerts_ReturnsWarningAlert_ForEquipmentDueWithinSevenDays()
    {
        var task = CreateTask("Soon Generator", EquipmentStatus.Healthy, 6);

        var alert = Assert.Single(_service.BuildAlerts([task]));

        Assert.Equal(MaintenanceAlertSeverity.Warning, alert.Severity);
        Assert.Equal("Soon Generator is due in 6 days.", alert.Message);
    }

    [Fact]
    public void BuildAlerts_OrdersCriticalAlertsBeforeWarningsAndInfo()
    {
        var alerts = _service.BuildAlerts(
        [
            CreateTask("Routine", EquipmentStatus.Healthy, 20),
            CreateTask("Soon", EquipmentStatus.Healthy, 5),
            CreateTask("Overdue", EquipmentStatus.Healthy, -1)
        ]);

        Assert.Equal(3, alerts.Count);
        Assert.Equal("Overdue", alerts[0].EquipmentName);
        Assert.Equal(MaintenanceAlertSeverity.Critical, alerts[0].Severity);
        Assert.Equal(MaintenanceAlertSeverity.Warning, alerts[1].Severity);
        Assert.Equal(MaintenanceAlertSeverity.Info, alerts[2].Severity);
    }

    [Fact]
    public void BuildSummary_ReturnsCountsAndHighestPriorityMessage()
    {
        var alerts = _service.BuildAlerts(
        [
            CreateTask("Soon", EquipmentStatus.Healthy, 5),
            CreateTask("Overdue", EquipmentStatus.Healthy, -2)
        ]);

        var summary = _service.BuildSummary(alerts);

        Assert.Equal("1 critical, 1 warning. Highest priority: Overdue is overdue by 2 days.", summary);
    }

    private static MaintenanceTask CreateTask(string name, EquipmentStatus status, int daysRemaining)
    {
        return new MaintenanceTask
        {
            EquipmentId = Guid.NewGuid(),
            EquipmentName = name,
            SerialNumber = $"{name.Replace(" ", "-", StringComparison.OrdinalIgnoreCase)}-001",
            Category = "Pump",
            DueDate = new DateTime(2026, 6, 26).AddDays(daysRemaining),
            DaysRemaining = daysRemaining,
            EquipmentStatus = status,
            PriorityLabel = daysRemaining < 0 ? "Immediate" : "Routine"
        };
    }
}
