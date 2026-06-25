# AeroMaintain

A Windows desktop application for tracking equipment health, scheduling maintenance, logging completed work, and guiding first-pass troubleshooting in industrial operations. Built as a portfolio project using C# 12, .NET 8, WPF, SQLite, and EF Core.

## Features

### Operations Dashboard
- Fleet snapshot: Healthy / Watch / Critical unit counts
- Average health score across the fleet
- Priority queue for overdue and critical items
- Visual distribution bars for current fleet status

### Equipment Registry
- Add, edit, delete, search, and import equipment records
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
- Audit trail for equipment create/update/delete events and maintenance-date changes
- Legacy `EnsureCreated()` databases are baselined before new migrations are applied

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
|   |   `-- AeroMaintainDbContextModelSnapshot.cs
|   |-- Models/
|   |   |-- AuditLog.cs
|   |   |-- Equipment.cs
|   |   |-- EquipmentStatus.cs
|   |   |-- HealthAssessment.cs
|   |   |-- MaintenanceLog.cs
|   |   |-- MaintenanceTask.cs
|   |   `-- TroubleshootingRule.cs
|   |-- Services/
|   |   |-- CsvImportService.cs
|   |   |-- DataService.cs
|   |   |-- ExportService.cs
|   |   |-- HealthScoreService.cs
|   |   |-- MaintenanceService.cs
|   |   `-- TroubleshootingService.cs
|   |-- MainWindow.xaml
|   `-- MainWindow.xaml.cs
`-- AeroMaintain.Tests/
    |-- DataServiceTests.cs
    |-- ExportServiceTests.cs
    |-- HealthScoreServiceTests.cs
    |-- MaintenanceServiceTests.cs
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

Current suite: 41 tests.

| Test class | Count | Coverage focus |
|---|---:|---|
| HealthScoreServiceTests | 9 | Scoring, penalty caps, labels, fleet averages |
| MaintenanceServiceTests | 8 | Due dates, priority labels, filters |
| TroubleshootingServiceTests | 4 | Symptom lookup behavior |
| DataServiceTests | 8 | SQLite persistence, migrations, legacy upgrade path, audit entries, maintenance history |
| ExportServiceTests | 11 | CSV and text report generation |

## Manual Smoke Test

1. Open the dashboard and confirm summary counts populate.
2. Search the Equipment Registry by name, serial number, and category.
3. Select a record, fill maintenance completion details, click **Log Maintenance (Today)**, then verify the next due date and History and Audit tab update.
4. Add a new equipment item, save, restart the app, and confirm it reloads.
5. Switch to Maintenance Scheduler and cycle through status and due-date filters.
6. Open Guided Troubleshooting and cycle through symptoms.
7. Generate a report and export both CSV files.
