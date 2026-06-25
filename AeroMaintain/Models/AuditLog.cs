namespace AeroMaintain.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public string UserName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Description { get; set; } = string.Empty;
}
