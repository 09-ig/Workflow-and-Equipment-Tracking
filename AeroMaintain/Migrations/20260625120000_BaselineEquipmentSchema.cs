using System;
using AeroMaintain.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroMaintain.Migrations;

[DbContext(typeof(AeroMaintainDbContext))]
[Migration("20260625120000_BaselineEquipmentSchema")]
public partial class BaselineEquipmentSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Equipment",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                SerialNumber = table.Column<string>(type: "TEXT", nullable: false),
                Category = table.Column<string>(type: "TEXT", nullable: false),
                LastMaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                MaintenanceIntervalDays = table.Column<int>(type: "INTEGER", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                RecentIssueCount = table.Column<int>(type: "INTEGER", nullable: false),
                Notes = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Equipment", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TroubleshootingRules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Symptom = table.Column<string>(type: "TEXT", nullable: false),
                PossibleCauses = table.Column<string>(type: "TEXT", nullable: false),
                RecommendedChecks = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TroubleshootingRules", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TroubleshootingRules");
        migrationBuilder.DropTable(name: "Equipment");
    }
}
