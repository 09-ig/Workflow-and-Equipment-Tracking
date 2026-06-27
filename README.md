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
| Seed data | JSON files in `AeroMaintain/Data` |
| Unit testing | xUnit 2.9 with coverlet |
| Build tooling | .NET SDK 8 |

## Project Structure

```text
C# project/
|-- AeroMaintain/
|   |-- Data/
|   |   |-- AeroMaintainDbContext.cs
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

The app stores its local database under the user's LocalAppData `AeroMaintain` folder. On first run, demo equipment and troubleshooting data are seeded from JSON.

## Running Tests

```powershell
dotnet test AeroMaintain.Tests
```

Current suite: 50 tests.

| Test class | Count | Coverage focus |
|---|---:|---|
| HealthScoreServiceTests | 9 | Scoring, penalty caps, labels, fleet averages |
| MaintenanceServiceTests | 8 | Due dates, priority labels, filters |
| NotificationServiceTests | 5 | Alert severity, ordering, summary text |
| PermissionServiceTests | 4 | Role-permission matrix and role descriptions |
| TroubleshootingServiceTests | 4 | Symptom lookup behavior |
| DataServiceTests | 8 | SQLite persistence, migrations, legacy upgrade path, audit entries, maintenance history |
| ExportServiceTests | 11 | CSV and text report generation |

## Manual Smoke Test

1. Open the dashboard and confirm summary counts populate.
2. Change the session role in the header and confirm buttons enable/disable by role.
3. Review Operational Notifications and click **Show Desktop Notification**.
4. Search the Equipment Registry by name, serial number, and category.
5. As Technician, select a record and log maintenance.
6. As Supervisor, add or edit equipment and verify the audit role column.
7. As Admin, confirm delete access is available.
8. Switch to Maintenance Scheduler and cycle through status and due-date filters.
9. Open Guided Troubleshooting and cycle through symptoms.
10. Generate a report and export both CSV files.
