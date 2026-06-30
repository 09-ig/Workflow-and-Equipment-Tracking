using System;
using AeroMaintain.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMaintain.Migrations;

[DbContext(typeof(AeroMaintainDbContext))]
[Migration("20260629120000_AddMaintenanceAssignments")]
public partial class AddMaintenanceAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MaintenanceAssignments",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                EquipmentId = table.Column<Guid>(nullable: false),
                EquipmentName = table.Column<string>(nullable: false),
                SerialNumber = table.Column<string>(nullable: false),
                AssignedTo = table.Column<string>(nullable: false),
                AssignedBy = table.Column<string>(nullable: false),
                DueDate = table.Column<DateTime>(nullable: false),
                Priority = table.Column<string>(nullable: false),
                WorkSummary = table.Column<string>(nullable: false),
                Notes = table.Column<string>(nullable: false),
                Status = table.Column<string>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                ApprovedBy = table.Column<string>(nullable: false),
                ApprovedAtUtc = table.Column<DateTime>(nullable: true),
                CompletedAtUtc = table.Column<DateTime>(nullable: true),
                CompletionMaintenanceLogId = table.Column<Guid>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MaintenanceAssignments", x => x.Id);
                table.ForeignKey(
                    name: "FK_MaintenanceAssignments_Equipment_EquipmentId",
                    column: x => x.EquipmentId,
                    principalTable: "Equipment",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_MaintenanceAssignments_EquipmentId_Status_DueDate",
            table: "MaintenanceAssignments",
            columns: new[] { "EquipmentId", "Status", "DueDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MaintenanceAssignments");
    }
}
