using System.Text.Json.Serialization;

namespace AeroMaintain.Models;

public class Equipment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime LastMaintenanceDate { get; set; } = DateTime.Today;
    public int MaintenanceIntervalDays { get; set; } = 30;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Healthy;
    public int RecentIssueCount { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime NextDueDate => LastMaintenanceDate.AddDays(MaintenanceIntervalDays);

    [JsonIgnore]
    public int DaysToDue => (NextDueDate - DateTime.Today).Days;

    [JsonIgnore]
    public bool IsOverdue => DaysToDue < 0;

    [JsonIgnore]
    public int HealthScore { get; set; }

    [JsonIgnore]
    public string HealthLabel { get; set; } = "Stable";
}
