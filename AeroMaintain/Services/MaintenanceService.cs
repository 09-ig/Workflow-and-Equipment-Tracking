using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class MaintenanceService
{
    public List<MaintenanceTask> BuildTasks(IEnumerable<Equipment> equipment, DateTime referenceDate)
    {
        var tasks = equipment.Select(e =>
            {
                var dueDate = e.LastMaintenanceDate.AddDays(e.MaintenanceIntervalDays);
                var daysRemaining = (dueDate - referenceDate.Date).Days;
                return new MaintenanceTask
                {
                    EquipmentId = e.Id,
                    EquipmentName = e.Name,
                    SerialNumber = e.SerialNumber,
                    Category = e.Category,
                    DueDate = dueDate,
                    DaysRemaining = daysRemaining,
                    EquipmentStatus = e.Status,
                    PriorityLabel = GetPriority(daysRemaining, e.Status)
                };
            })
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.EquipmentStatus)
            .ToList();

        return tasks;
    }

    public List<MaintenanceTask> ApplyFilters(
        IEnumerable<MaintenanceTask> tasks,
        string statusFilter,
        string dueFilter,
        DateTime referenceDate)
    {
        var filtered = tasks;

        if (!string.Equals(statusFilter, "All", StringComparison.OrdinalIgnoreCase)
            && Enum.TryParse<EquipmentStatus>(statusFilter, out var status))
        {
            filtered = filtered.Where(t => t.EquipmentStatus == status);
        }

        filtered = dueFilter switch
        {
            "Overdue" => filtered.Where(t => t.IsOverdue),
            "Due within 7 days" => filtered.Where(t => t.DaysRemaining >= 0 && t.DaysRemaining <= 7),
            "Due within 30 days" => filtered.Where(t => t.DaysRemaining >= 0 && t.DaysRemaining <= 30),
            _ => filtered
        };

        return filtered
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.EquipmentName)
            .ToList();
    }

    private static string GetPriority(int daysRemaining, EquipmentStatus status)
    {
        if (daysRemaining < 0 || status == EquipmentStatus.Critical)
        {
            return "Immediate";
        }

        if (daysRemaining <= 7 || status == EquipmentStatus.Watch)
        {
            return "Monitor";
        }

        return "Routine";
    }
}
