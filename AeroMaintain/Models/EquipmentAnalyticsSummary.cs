namespace AeroMaintain.Models;

public class EquipmentAnalyticsSummary
{
    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public EquipmentStatus Status { get; set; }
    public int MaintenanceEventCount { get; set; }
    public double AverageDaysBetweenMaintenance { get; set; }
    public double AverageRepairHours { get; set; }
    public double TotalLaborHours { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime? LastMaintenanceCompletedOn { get; set; }
    public int DaysUntilDue { get; set; }
    public int RecentIssueCount { get; set; }
    public string ReliabilityLabel { get; set; } = string.Empty;
}
