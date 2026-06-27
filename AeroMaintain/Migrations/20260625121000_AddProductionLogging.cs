using System;
using AeroMaintain.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMaintain.Migrations;

[DbContext(typeof(AeroMaintainDbContext))]
[Migration("20260625121000_AddProductionLogging")]
public partial class AddProductionLogging : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                ChangedAtUtc = table.Column<DateTime>(nullable: false),
                UserName = table.Column<string>(nullable: false),
                EntityName = table.Column<string>(nullable: false),
                EntityId = table.Column<Guid>(nullable: true),
                Action = table.Column<string>(nullable: false),
                FieldName = table.Column<string>(nullable: false),
                OldValue = table.Column<string>(nullable: true),
                NewValue = table.Column<string>(nullable: true),
                Description = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "MaintenanceLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                EquipmentId = table.Column<Guid>(nullable: false),
                EquipmentName = table.Column<string>(nullable: false),
                CompletedOn = table.Column<DateTime>(nullable: false),
                PerformedBy = table.Column<string>(nullable: false),
                WorkSummary = table.Column<string>(nullable: false),
                PartsReplaced = table.Column<string>(nullable: false),
                Cost = table.Column<decimal>(nullable: false),
                LaborHours = table.Column<double>(nullable: false),
                Notes = table.Column<string>(nullable: false),
                LoggedAtUtc = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MaintenanceLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_MaintenanceLogs_Equipment_EquipmentId",
                    column: x => x.EquipmentId,
                    principalTable: "Equipment",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_ChangedAtUtc",
            table: "AuditLogs",
            column: "ChangedAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_EntityName_EntityId",
            table: "AuditLogs",
            columns: new[] { "EntityName", "EntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_MaintenanceLogs_EquipmentId_CompletedOn",
            table: "MaintenanceLogs",
            columns: new[] { "EquipmentId", "CompletedOn" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLogs");
        migrationBuilder.DropTable(name: "MaintenanceLogs");
    }
}
