using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class NotificationService
{
    public List<MaintenanceAlert> BuildAlerts(IEnumerable<MaintenanceTask> tasks)
    {
        return tasks
            .Select(BuildAlert)
            .Where(alert => alert is not null)
            .Cast<MaintenanceAlert>()
            .OrderByDescending(alert => alert.Severity)
            .ThenBy(alert => alert.DaysRemaining)
            .ThenBy(alert => alert.EquipmentName)
            .ToList();
    }

    public string BuildSummary(IEnumerable<MaintenanceAlert> alerts)
    {
        var items = alerts.ToList();
        if (items.Count == 0)
        {
            return "No active operational notifications.";
        }

        var criticalCount = items.Count(alert => alert.Severity == MaintenanceAlertSeverity.Critical);
        var warningCount = items.Count(alert => alert.Severity == MaintenanceAlertSeverity.Warning);
        var infoCount = items.Count(alert => alert.Severity == MaintenanceAlertSeverity.Info);
        var highest = items[0];

        var parts = new List<string>();
        if (criticalCount > 0) parts.Add($"{criticalCount} critical");
        if (warningCount > 0) parts.Add($"{warningCount} warning");
        if (infoCount > 0) parts.Add($"{infoCount} info");

        return $"{string.Join(", ", parts)}. Highest priority: {highest.Message}";
    }

    private static MaintenanceAlert? BuildAlert(MaintenanceTask task)
    {
        if (task.IsOverdue || task.EquipmentStatus == EquipmentStatus.Critical)
        {
            return CreateAlert(
                task,
                MaintenanceAlertSeverity.Critical,
                BuildCriticalMessage(task),
                "Assign immediate maintenance");
        }

        if (task.DaysRemaining <= 7 || task.EquipmentStatus == EquipmentStatus.Watch)
        {
            return CreateAlert(
                task,
                MaintenanceAlertSeverity.Warning,
                BuildWarningMessage(task),
                "Review during shift planning");
        }

        if (task.DaysRemaining <= 30)
        {
            return CreateAlert(
                task,
                MaintenanceAlertSeverity.Info,
                $"{task.EquipmentName} is due in {task.DaysRemaining} days.",
                "Schedule upcoming work");
        }

        return null;
    }

    private static MaintenanceAlert CreateAlert(
        MaintenanceTask task,
        MaintenanceAlertSeverity severity,
        string message,
        string actionLabel)
    {
        return new MaintenanceAlert
        {
            EquipmentId = task.EquipmentId,
            EquipmentName = task.EquipmentName,
            SerialNumber = task.SerialNumber,
            Category = task.Category,
            DueDate = task.DueDate,
            DaysRemaining = task.DaysRemaining,
            EquipmentStatus = task.EquipmentStatus,
            Severity = severity,
            Message = message,
            ActionLabel = actionLabel
        };
    }

    private static string BuildCriticalMessage(MaintenanceTask task)
    {
        if (task.IsOverdue && task.EquipmentStatus == EquipmentStatus.Critical)
        {
            return $"{task.EquipmentName} is critical and overdue by {Math.Abs(task.DaysRemaining)} days.";
        }

        if (task.IsOverdue)
        {
            return $"{task.EquipmentName} is overdue by {Math.Abs(task.DaysRemaining)} days.";
        }

        return $"{task.EquipmentName} is marked Critical and due {FormatDueWindow(task.DaysRemaining)}.";
    }

    private static string BuildWarningMessage(MaintenanceTask task)
    {
        if (task.EquipmentStatus == EquipmentStatus.Watch && task.DaysRemaining > 7)
        {
            return $"{task.EquipmentName} is on the watchlist and due in {task.DaysRemaining} days.";
        }

        return $"{task.EquipmentName} is due {FormatDueWindow(task.DaysRemaining)}.";
    }

    private static string FormatDueWindow(int daysRemaining)
    {
        return daysRemaining switch
        {
            0 => "today",
            1 => "in 1 day",
            _ => $"in {daysRemaining} days"
        };
    }
}
