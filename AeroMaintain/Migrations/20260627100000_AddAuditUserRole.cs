using AeroMaintain.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMaintain.Migrations;

[DbContext(typeof(AeroMaintainDbContext))]
[Migration("20260627100000_AddAuditUserRole")]
public partial class AddAuditUserRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "UserRole",
            table: "AuditLogs",
            type: "TEXT",
            nullable: false,
            defaultValue: "Admin");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UserRole",
            table: "AuditLogs");
    }
}
