namespace AeroMaintain.Models;

public class MaintenanceAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.Today;
    public string Priority { get; set; } = "Routine";
    public string WorkSummary { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public MaintenanceAssignmentStatus Status { get; set; } = MaintenanceAssignmentStatus.Assigned;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public Guid? CompletionMaintenanceLogId { get; set; }

    public Equipment? Equipment { get; set; }

    public bool IsOpen =>
        Status is MaintenanceAssignmentStatus.Assigned or MaintenanceAssignmentStatus.Approved;
}
