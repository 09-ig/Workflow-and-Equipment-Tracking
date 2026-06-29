using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class AnalyticsService
{
    public AnalyticsSnapshot BuildSnapshot(
        IEnumerable<Equipment> equipment,
        IEnumerable<MaintenanceLog> maintenanceLogs,
        IEnumerable<MaintenanceTask> tasks,
        DateTime referenceDate)
    {
        var equipmentList = equipment.ToList();
        var logList = maintenanceLogs.ToList();
        var taskList = tasks.ToList();
        var logsByEquipment = logList
            .GroupBy(log => log.EquipmentId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var tasksByEquipment = taskList.ToDictionary(task => task.EquipmentId);

        var equipmentSummaries = equipmentList
            .Select(item => BuildEquipmentSummary(
                item,
                logsByEquipment.GetValueOrDefault(item.Id) ?? [],
                tasksByEquipment.GetValueOrDefault(item.Id),
                referenceDate))
            .OrderByDescending(summary => summary.TotalCost)
            .ThenBy(summary => summary.DaysUntilDue)
            .ThenBy(summary => summary.EquipmentName)
            .ToList();

        var categorySummaries = BuildCategorySummaries(equipmentSummaries);
        var totalCost = logList.Sum(log => log.Cost);
        var totalLaborHours = logList.Sum(log => log.LaborHours);
        var eventCount = logList.Count;
        var fleetAverageDays = AverageNonZero(equipmentSummaries.Select(summary => summary.AverageDaysBetweenMaintenance));
        var averageRepairHours = eventCount == 0 ? 0 : totalLaborHours / eventCount;

        return new AnalyticsSnapshot
        {
            EquipmentCount = equipmentList.Count,
            MaintenanceEventCount = eventCount,
            FleetAverageDaysBetweenMaintenance = fleetAverageDays,
            AverageRepairHours = averageRepairHours,
            TotalLaborHours = totalLaborHours,
            TotalMaintenanceCost = totalCost,
            AverageCostPerEvent = eventCount == 0 ? 0 : totalCost / eventCount,
            OverdueCount = taskList.Count(task => task.IsOverdue),
            CriticalCount = equipmentList.Count(item => item.Status == EquipmentStatus.Critical),
            UpcomingThirtyDayCount = taskList.Count(task => !task.IsOverdue && task.DaysRemaining <= 30),
            ReliabilityNote = BuildReliabilityNote(equipmentList.Count, eventCount, fleetAverageDays),
            TopCostDriver = BuildTopCostDriver(equipmentSummaries),
            EquipmentSummaries = equipmentSummaries,
            CategorySummaries = categorySummaries
        };
    }

    private static EquipmentAnalyticsSummary BuildEquipmentSummary(
        Equipment equipment,
        IReadOnlyCollection<MaintenanceLog> logs,
        MaintenanceTask? task,
        DateTime referenceDate)
    {
        var orderedLogs = logs
            .OrderBy(log => log.CompletedOn)
            .ToList();
        var eventCount = orderedLogs.Count;
        var totalCost = orderedLogs.Sum(log => log.Cost);
        var totalLaborHours = orderedLogs.Sum(log => log.LaborHours);
        var averageDays = CalculateAverageDaysBetweenMaintenance(orderedLogs);
        var averageRepairHours = eventCount == 0 ? 0 : totalLaborHours / eventCount;
        var daysUntilDue = task?.DaysRemaining ?? (equipment.NextDueDate - referenceDate.Date).Days;
        var isOverdue = task?.IsOverdue ?? daysUntilDue < 0;

        return new EquipmentAnalyticsSummary
        {
            EquipmentId = equipment.Id,
            EquipmentName = equipment.Name,
            SerialNumber = equipment.SerialNumber,
            Category = equipment.Category,
            Status = equipment.Status,
            MaintenanceEventCount = eventCount,
            AverageDaysBetweenMaintenance = averageDays,
            AverageRepairHours = averageRepairHours,
            TotalLaborHours = totalLaborHours,
            TotalCost = totalCost,
            LastMaintenanceCompletedOn = orderedLogs.LastOrDefault()?.CompletedOn,
            DaysUntilDue = daysUntilDue,
            RecentIssueCount = equipment.RecentIssueCount,
            ReliabilityLabel = BuildReliabilityLabel(equipment, eventCount, averageDays, isOverdue)
        };
    }

    private static List<CategoryAnalyticsSummary> BuildCategorySummaries(
        IReadOnlyCollection<EquipmentAnalyticsSummary> equipmentSummaries)
    {
        return equipmentSummaries
            .GroupBy(summary => string.IsNullOrWhiteSpace(summary.Category) ? "Uncategorized" : summary.Category)
            .Select(group =>
            {
                var items = group.ToList();
                var eventCount = items.Sum(item => item.MaintenanceEventCount);
                var totalCost = items.Sum(item => item.TotalCost);
                var totalLaborHours = items.Sum(item => item.TotalLaborHours);
                var criticalCount = items.Count(item => item.Status == EquipmentStatus.Critical);
                var overdueCount = items.Count(item => item.DaysUntilDue < 0);

                return new CategoryAnalyticsSummary
                {
                    Category = group.Key,
                    EquipmentCount = items.Count,
                    MaintenanceEventCount = eventCount,
                    AverageDaysBetweenMaintenance = AverageNonZero(items.Select(item => item.AverageDaysBetweenMaintenance)),
                    AverageRepairHours = eventCount == 0 ? 0 : totalLaborHours / eventCount,
                    TotalLaborHours = totalLaborHours,
                    TotalCost = totalCost,
                    AverageCostPerEvent = eventCount == 0 ? 0 : totalCost / eventCount,
                    CriticalCount = criticalCount,
                    OverdueCount = overdueCount,
                    RiskLabel = BuildCategoryRiskLabel(eventCount, criticalCount, overdueCount)
                };
            })
            .OrderByDescending(summary => summary.TotalCost)
            .ThenByDescending(summary => summary.OverdueCount)
            .ThenBy(summary => summary.Category)
            .ToList();
    }

    private static double CalculateAverageDaysBetweenMaintenance(IReadOnlyList<MaintenanceLog> orderedLogs)
    {
        if (orderedLogs.Count < 2)
        {
            return 0;
        }

        var intervals = new List<double>();
        for (var i = 1; i < orderedLogs.Count; i++)
        {
            intervals.Add((orderedLogs[i].CompletedOn.Date - orderedLogs[i - 1].CompletedOn.Date).TotalDays);
        }

        return intervals.Count == 0 ? 0 : intervals.Average();
    }

    private static double AverageNonZero(IEnumerable<double> values)
    {
        var usableValues = values
            .Where(value => value > 0)
            .ToList();

        return usableValues.Count == 0 ? 0 : usableValues.Average();
    }

    private static string BuildReliabilityLabel(
        Equipment equipment,
        int eventCount,
        double averageDaysBetweenMaintenance,
        bool isOverdue)
    {
        if (isOverdue || equipment.Status == EquipmentStatus.Critical)
        {
            return "At Risk";
        }

        if (eventCount == 0)
        {
            return "No History";
        }

        if (eventCount == 1)
        {
            return "Building History";
        }

        if (averageDaysBetweenMaintenance < 30 || equipment.RecentIssueCount >= 4)
        {
            return "High Touch";
        }

        if (averageDaysBetweenMaintenance >= 90)
        {
            return "Stable";
        }

        return "Normal";
    }

    private static string BuildCategoryRiskLabel(int eventCount, int criticalCount, int overdueCount)
    {
        if (criticalCount > 0 || overdueCount > 0)
        {
            return "Immediate";
        }

        return eventCount == 0 ? "No History" : "Normal";
    }

    private static string BuildReliabilityNote(int equipmentCount, int eventCount, double fleetAverageDays)
    {
        if (equipmentCount == 0)
        {
            return "Add equipment and log maintenance to activate analytics.";
        }

        if (eventCount == 0)
        {
            return "No completed maintenance records yet. Log work orders to calculate trends.";
        }

        if (fleetAverageDays <= 0)
        {
            return "More history is needed for MTBF. MTTR and cost totals are available.";
        }

        return $"Fleet average days between completed maintenance: {fleetAverageDays:F1}.";
    }

    private static string BuildTopCostDriver(IReadOnlyCollection<EquipmentAnalyticsSummary> equipmentSummaries)
    {
        var topCost = equipmentSummaries
            .Where(summary => summary.TotalCost > 0)
            .OrderByDescending(summary => summary.TotalCost)
            .FirstOrDefault();

        return topCost is null
            ? "No maintenance costs recorded yet."
            : $"{topCost.EquipmentName} leads cost at {topCost.TotalCost:C0}.";
    }
}
