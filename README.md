# AeroMaintain

A Windows desktop application for tracking equipment health, scheduling maintenance, logging completed work, and guiding first-pass troubleshooting in industrial operations. Built as a portfolio project using C# 12, .NET 8, WPF, SQLite, and EF Core.

## Features

### Operations Dashboard
- Fleet snapshot: Healthy / Watch / Critical unit counts
- Average health score across the fleet
- Priority queue for overdue and critical items
- Visual distribution bars for current fleet status
- Operational notifications for critical, overdue, due-soon, and watchlist equipment
- Windows tray-style desktop alert for the current notification set

### Equipment Registry
- Add, edit, delete, search, and import equipment records
- Role-based controls for technician, supervisor, and admin workflows
- Duplicate serial number validation
- Auto-computed next due date, health score, and health label
- SQLite persistence with EF Core migrations
- Optional SQL Server Express/shared database mode
- Demo JSON seed data loaded on first run

### Maintenance Scheduler
- Task list generated from last maintenance date and interval
- Status and due-window filters
- Overdue highlighting
- Priority labels: Immediate / Monitor / Routine
- CSV export for filtered task views

### Maintenance History and Audit
- Maintenance completion log with technician, work summary, parts, cost, labor hours, and notes
- Per-equipment history focus when a registry row is selected
- Audit trail for equipment create/update/delete events and maintenance-date changes, including username and active role
- Legacy `EnsureCreated()` databases are baselined before new migrations are applied

### Identity and Permissions
- Current Windows user shown in the app header
- Session role selector for local testing: `Technician`, `Supervisor`, `Admin`
- Technician: view equipment and log maintenance
- Supervisor: edit/import/export equipment data and review audit history
- Admin: full access, including equipment deletion and configuration permissions

### Guided Troubleshooting
- Rule-based lookup for common symptoms
- Possible causes and recommended checks for first-pass inspection
- Supervisor/Admin editor for troubleshooting symptoms, causes, and checks
- Troubleshooting rule create/update/delete events are audit logged

### Reports and Export
- Equipment CSV export
- Maintenance task CSV export
- Text report generation for handovers

## Tech Stack

| Layer | Technology |
|---|---|
| UI framework | WPF (.NET 8, Windows) |
| Desktop alerts | Windows Forms `NotifyIcon` balloon notifications |
| Language | C# 12 |
| Data storage | SQLite via EF Core migrations |
| Shared DB option | SQL Server / SQL Server Express via EF Core provider |
| Seed data | JSON files in `AeroMaintain/Data` |
| Unit testing | xUnit 2.9 with coverlet |
| Build tooling | .NET SDK 8 |

## Project Structure

```text
C# project/
|-- AeroMaintain/
|   |-- Data/
|   |   |-- AeroMaintainDbContext.cs
|   |   |-- DatabaseProvider.cs
|   |   |-- DatabaseSettings.cs
|   |   |-- database.settings.example.json
|   |   |-- equipment.json
|   |   `-- troubleshooting_rules.json
|   |-- Migrations/
|   |   |-- 20260625120000_BaselineEquipmentSchema.cs
|   |   |-- 20260625121000_AddProductionLogging.cs
|   |   |-- 20260627100000_AddAuditUserRole.cs
|   |   `-- AeroMaintainDbContextModelSnapshot.cs
|   |-- Models/
|   |   |-- AuditLog.cs
|   |   |-- Equipment.cs
|   |   |-- EquipmentStatus.cs
|   |   |-- HealthAssessment.cs
|   |   |-- MaintenanceLog.cs
|   |   |-- MaintenanceAlert.cs
|   |   |-- MaintenanceAlertSeverity.cs
|   |   |-- MaintenanceTask.cs
|   |   |-- UserPermission.cs
|   |   |-- UserRole.cs
|   |   `-- TroubleshootingRule.cs
|   |-- Services/
|   |   |-- CsvImportService.cs
|   |   |-- DataService.cs
|   |   |-- ExportService.cs
|   |   |-- HealthScoreService.cs
|   |   |-- MaintenanceService.cs
|   |   |-- NotificationService.cs
|   |   |-- PermissionService.cs
|   |   |-- DesktopNotificationService.cs
|   |   `-- TroubleshootingService.cs
|   |-- MainWindow.xaml
|   `-- MainWindow.xaml.cs
`-- AeroMaintain.Tests/
    |-- DataServiceTests.cs
    |-- ExportServiceTests.cs
    |-- HealthScoreServiceTests.cs
    |-- MaintenanceServiceTests.cs
    |-- NotificationServiceTests.cs
    |-- PermissionServiceTests.cs
    `-- TroubleshootingServiceTests.cs
```

## Application Flow

```text
JSON seed files
    |
    v
SQLite + EF Core migrations -> DataService -> MainWindow
                                      |
                                      |-- HealthScoreService
                                      |-- MaintenanceService
                                      |-- NotificationService
                                      |-- PermissionService
                                      |-- TroubleshootingService
                                      `-- ExportService
```

## Build and Run

```powershell
dotnet restore
dotnet run --project AeroMaintain

dotnet build AeroMaintain -c Release
```

The app stores its local SQLite database under the user's LocalAppData `AeroMaintain` folder. On first run, demo equipment and troubleshooting data are seeded from JSON.

## Shared Database Mode

SQLite remains the default. To point the app at SQL Server Express, create this file:

```text
%LocalAppData%\AeroMaintain\database.settings.json
```

Use this content:

```json
{
  "Provider": "SqlServer",
  "ConnectionString": "Server=.\\SQLEXPRESS;Database=AeroMaintain;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

You can also use environment variables:

```powershell
$env:AEROMAINTAIN_DB_PROVIDER = "SqlServer"
$env:AEROMAINTAIN_CONNECTION_STRING = "Server=.\SQLEXPRESS;Database=AeroMaintain;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
dotnet run --project AeroMaintain
```

The header shows the active database provider and where settings were loaded from.

## Running Tests

```powershell
dotnet test AeroMaintain.Tests
```

Current suite: 55 tests.

| Test class | Count | Coverage focus |
|---|---:|---|
| DatabaseSettingsTests | 3 | SQLite/SQL Server configuration helpers |
| HealthScoreServiceTests | 9 | Scoring, penalty caps, labels, fleet averages |
| MaintenanceServiceTests | 8 | Due dates, priority labels, filters |
| NotificationServiceTests | 5 | Alert severity, ordering, summary text |
| PermissionServiceTests | 4 | Role-permission matrix and role descriptions |
| TroubleshootingServiceTests | 5 | Symptom lookup behavior and multiline editor parsing |
| DataServiceTests | 9 | SQLite persistence, migrations, legacy upgrade path, audit entries, maintenance history, troubleshooting rule sync |
| ExportServiceTests | 11 | CSV and text report generation |

## Manual Smoke Test

1. Open the dashboard and confirm summary counts populate.
2. Confirm the header shows the active database provider.
3. Change the session role in the header and confirm buttons enable/disable by role.
4. Review Operational Notifications and click **Show Desktop Notification**.
5. Search the Equipment Registry by name, serial number, and category.
6. As Technician, select a record and log maintenance.
7. As Supervisor, add or edit equipment and verify the audit role column.
8. As Admin, confirm delete access is available.
9. Switch to Maintenance Scheduler and cycle through status and due-date filters.
10. Open Guided Troubleshooting, edit a rule as Supervisor/Admin, and verify the audit entry.
11. Switch to Technician and confirm troubleshooting rule editing is disabled.
12. Generate a report and export both CSV files.
