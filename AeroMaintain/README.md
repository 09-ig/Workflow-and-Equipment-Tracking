# AeroMaintain
**Tagline:** Practical maintenance workflow and equipment health tracking for student-built industrial software.

## Project Summary
AeroMaintain is a Windows desktop app built in C#/.NET WPF to simulate how engineering teams track equipment condition, schedule maintenance, and perform guided troubleshooting.  
It focuses on **practical operations clarity**, not flashy UI trends.

## Why I Built It
I built this project to demonstrate software engineering and data-thinking skills relevant to industrial and aerospace internships:
- structured equipment records
- maintenance planning logic
- rule-based troubleshooting
- health scoring and operational analytics
- exportable reports for shift handover or review

## Features
- Equipment registry with:
  - machine name, serial number, category
  - last maintenance date and interval
  - status (`Healthy`, `Watch`, `Critical`)
  - recent issue count and notes
- Maintenance scheduler:
  - auto-calculated next due date
  - overdue highlighting
  - status and due-window filters
- Guided troubleshooting:
  - symptom-based rule lookup
  - possible causes and recommended checks
- Health scoring:
  - 0–100 score
  - labels: `Stable`, `Needs Attention`, `Immediate Check Required`
- Operations dashboard:
  - healthy/watch/critical counts
  - overdue and upcoming task counts
  - average health score
  - at-risk queue
- Export and reporting:
  - equipment CSV export
  - maintenance task CSV export
  - text report generation

## Tech Stack
- C#
- .NET 8
- WPF (Windows desktop)
- Local JSON data storage (`Data/equipment.json`, `Data/troubleshooting_rules.json`)
- CSV and TXT export via standard .NET I/O

## UI Design Direction
- Grounded industrial palette (graphite, slate, off-white, muted amber/steel/olive accents)
- Readable typography and spacing for operator-style workflows
- Practical labels and microcopy for real usage context
- Empty-state and operational guidance messaging

## Screenshots
Add screenshots in `Assets/Screenshots/` and reference them here:
- `dashboard-overview.png`
- `equipment-registry.png`
- `maintenance-scheduler.png`
- `troubleshooting-module.png`
- `report-export.png`

## Architecture Overview
`MainWindow` handles UI orchestration while business logic is separated into services:
- `Models/`: domain entities (`Equipment`, `MaintenanceTask`, `TroubleshootingRule`, `HealthAssessment`)
- `Services/`:
  - `DataService`: JSON load/save
  - `MaintenanceService`: due-date and priority task logic
  - `HealthScoreService`: health scoring and labels
  - `TroubleshootingService`: rule lookup
  - `ExportService`: CSV/TXT output

## Folder Structure
```text
AeroMaintain/
├── AeroMaintain.csproj
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Models/
│   ├── Equipment.cs
│   ├── EquipmentStatus.cs
│   ├── HealthAssessment.cs
│   ├── MaintenanceTask.cs
│   └── TroubleshootingRule.cs
├── Services/
│   ├── DataService.cs
│   ├── ExportService.cs
│   ├── HealthScoreService.cs
│   ├── MaintenanceService.cs
│   └── TroubleshootingService.cs
├── Data/
│   ├── equipment.json
│   └── troubleshooting_rules.json
├── Assets/
│   └── Screenshots/
│       └── .gitkeep
├── .gitignore
└── README.md
```

## How to Run Locally
1. Install .NET 8 SDK for Windows.
2. Open a terminal in the project directory.
3. Run:
   ```powershell
   dotnet restore
   dotnet run --project AeroMaintain.csproj
   ```
4. On first run, sample data is loaded from `Data/equipment.json`.

## Sample Use Cases
- A maintenance engineer reviews overdue units before shift start.
- A student team member logs a new compressor and sets a 60-day interval.
- A technician uses symptom lookup (`Vibration`) to follow first-pass checks.
- A supervisor exports task CSV for weekly maintenance planning.
- A text report is generated for handover summary.

## Implementation Plan (Build Sequence)
1. Scaffold WPF project and define domain models.
2. Implement JSON data storage service for local persistence.
3. Implement maintenance scheduler calculations and filters.
4. Implement health scoring algorithm and dashboard metrics.
5. Implement troubleshooting rule engine and symptom UI.
6. Implement CSV and text export services.
7. Build UI tabs for dashboard, registry, scheduler, troubleshooting, reports.
8. Add realistic sample data and polish microcopy.

## What I Learned
- Translating maintenance workflow ideas into clean desktop software architecture.
- Balancing beginner-friendly implementation with professional separation of concerns.
- Designing grounded industrial UI decisions for practical software contexts.
- Building explainable scoring and troubleshooting rules that are easy to audit.

## Future Improvements
- SQLite storage + migration from JSON.
- Equipment history timeline and maintenance event log.
- Search and sort enhancements in large fleets.
- Role-based views (operator vs supervisor).
- Trend charts for health score over time.
- Notification reminders for upcoming due tasks.
- Import equipment from CSV.

## Resume Bullet Points
- Built **AeroMaintain**, a C#/.NET WPF desktop application simulating industrial maintenance workflow with equipment registry, due-date scheduling, and rule-based troubleshooting.
- Implemented a maintenance planning engine that auto-calculates next service dates, flags overdue items, and supports operational filtering by status and horizon windows.
- Designed an explainable health scoring system (0-100) using maintenance delay, equipment status, and issue frequency to prioritize intervention.
- Developed local data persistence and reporting features, including JSON-backed records, CSV exports, and text-based shift summary generation.
- Delivered a portfolio-quality, operator-focused UI with realistic maintenance terminology, improving clarity for technical users versus generic dashboard templates.
