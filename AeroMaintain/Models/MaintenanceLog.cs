namespace AeroMaintain.Models;

public class MaintenanceLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public DateTime CompletedOn { get; set; } = DateTime.Today;
    public string PerformedBy { get; set; } = string.Empty;
    public string WorkSummary { get; set; } = string.Empty;
    public string PartsReplaced { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public double LaborHours { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;

    public Equipment? Equipment { get; set; }
}
