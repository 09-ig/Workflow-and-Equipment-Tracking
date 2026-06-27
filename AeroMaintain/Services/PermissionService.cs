using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class PermissionService
{
    public bool HasPermission(UserRole role, UserPermission permission)
    {
        return role switch
        {
            UserRole.Admin => true,
            UserRole.Supervisor => permission is
                UserPermission.ViewEquipment or
                UserPermission.LogMaintenance or
                UserPermission.EditEquipment or
                UserPermission.ImportEquipment or
                UserPermission.ExportData or
                UserPermission.ViewAuditTrail or
                UserPermission.ManageTroubleshooting,
            UserRole.Technician => permission is
                UserPermission.ViewEquipment or
                UserPermission.LogMaintenance,
            _ => false
        };
    }

    public string DescribeRole(UserRole role)
    {
        return role switch
        {
            UserRole.Technician => "Technician: view equipment and log completed work.",
            UserRole.Supervisor => "Supervisor: edit equipment, import records, export data, and review audit history.",
            UserRole.Admin => "Admin: full access including equipment deletion and configuration.",
            _ => "Unknown role."
        };
    }
}
