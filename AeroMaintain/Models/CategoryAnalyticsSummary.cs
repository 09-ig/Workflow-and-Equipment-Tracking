namespace AeroMaintain.Models;

public class CategoryAnalyticsSummary
{
    public string Category { get; set; } = string.Empty;
    public int EquipmentCount { get; set; }
    public int MaintenanceEventCount { get; set; }
    public double AverageDaysBetweenMaintenance { get; set; }
    public double AverageRepairHours { get; set; }
    public double TotalLaborHours { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCostPerEvent { get; set; }
    public int CriticalCount { get; set; }
    public int OverdueCount { get; set; }
    public string RiskLabel { get; set; } = string.Empty;
}
