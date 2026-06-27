using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class PermissionServiceTests
{
    private readonly PermissionService _service = new();

    [Fact]
    public void Technician_CanViewEquipmentAndLogMaintenanceOnly()
    {
        Assert.True(_service.HasPermission(UserRole.Technician, UserPermission.ViewEquipment));
        Assert.True(_service.HasPermission(UserRole.Technician, UserPermission.LogMaintenance));
        Assert.False(_service.HasPermission(UserRole.Technician, UserPermission.EditEquipment));
        Assert.False(_service.HasPermission(UserRole.Technician, UserPermission.DeleteEquipment));
        Assert.False(_service.HasPermission(UserRole.Technician, UserPermission.ViewAuditTrail));
    }

    [Fact]
    public void Supervisor_CanEditImportExportAndViewAudit_ButCannotDelete()
    {
        Assert.True(_service.HasPermission(UserRole.Supervisor, UserPermission.EditEquipment));
        Assert.True(_service.HasPermission(UserRole.Supervisor, UserPermission.ImportEquipment));
        Assert.True(_service.HasPermission(UserRole.Supervisor, UserPermission.ExportData));
        Assert.True(_service.HasPermission(UserRole.Supervisor, UserPermission.ViewAuditTrail));
        Assert.False(_service.HasPermission(UserRole.Supervisor, UserPermission.DeleteEquipment));
        Assert.False(_service.HasPermission(UserRole.Supervisor, UserPermission.ConfigureSystem));
    }

    [Fact]
    public void Admin_HasEveryPermission()
    {
        foreach (var permission in Enum.GetValues<UserPermission>())
        {
            Assert.True(_service.HasPermission(UserRole.Admin, permission));
        }
    }

    [Fact]
    public void DescribeRole_ReturnsOperatorFriendlyText()
    {
        var description = _service.DescribeRole(UserRole.Technician);

        Assert.Contains("Technician", description);
        Assert.Contains("log completed work", description);
    }
}
