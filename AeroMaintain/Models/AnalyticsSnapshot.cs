namespace AeroMaintain.Models;

public class AnalyticsSnapshot
{
    public int EquipmentCount { get; set; }
    public int MaintenanceEventCount { get; set; }
    public double FleetAverageDaysBetweenMaintenance { get; set; }
    public double AverageRepairHours { get; set; }
    public double TotalLaborHours { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public decimal AverageCostPerEvent { get; set; }
    public int OverdueCount { get; set; }
    public int CriticalCount { get; set; }
    public int UpcomingThirtyDayCount { get; set; }
    public string ReliabilityNote { get; set; } = string.Empty;
    public string TopCostDriver { get; set; } = string.Empty;
    public List<EquipmentAnalyticsSummary> EquipmentSummaries { get; set; } = new();
    public List<CategoryAnalyticsSummary> CategorySummaries { get; set; } = new();
}
