namespace AeroMaintain.Models;

public class MaintenanceTask
{
    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsOverdue => DaysRemaining < 0;
    public EquipmentStatus EquipmentStatus { get; set; }
    public string PriorityLabel { get; set; } = string.Empty;
}
