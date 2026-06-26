namespace AeroMaintain.Models;

public class MaintenanceAlert
{
    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysRemaining { get; set; }
    public EquipmentStatus EquipmentStatus { get; set; }
    public MaintenanceAlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
    public bool IsCritical => Severity == MaintenanceAlertSeverity.Critical;
}
