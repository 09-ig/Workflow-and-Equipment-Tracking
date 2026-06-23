# AeroMaintain

A Windows desktop application for tracking equipment health, scheduling maintenance, and guiding first-pass troubleshooting in industrial operations. Built as a portfolio project using C# 12, .NET 8, and WPF.

---

## Features

### Operations Dashboard
- Live fleet snapshot: Healthy / Watch / Critical unit counts at a glance
- Computed average health score across the entire fleet
- Priority queue highlighting overdue and critical items (top 8)
- Visual distribution bar showing fleet status balance

### Equipment Registry
- Add, edit, and delete equipment entries with full field validation
- Real-time search filter by name, serial number, or category
- One-click **Mark Maintained (Today)** resets the maintenance clock instantly
- Status badges with colour coding (green / amber / red)
- Auto-computed health score and label per unit
- Duplicate serial number guard on save
- JSON persistence — registry survives application restarts

### Maintenance Scheduler
- Builds a task list automatically from interval + last-service date
- Filter by equipment status (Healthy / Watch / Critical) and due window (Overdue, 7 days, 30 days)
- Overdue rows highlighted red for immediate visibility
- Priority labels: Immediate / Monitor / Routine
- Export the filtered task view to a timestamped CSV

### Guided Troubleshooting
- Rule-based symptom lookup across 8 common failure modes:  
  Overheating, Vibration, Delayed startup, Abnormal noise, Sensor warning, Pressure drop, Oil or fluid leak, Electrical fault
- Each symptom surfaces possible causes and a step-by-step check procedure
- Standardises first-pass inspection before escalation to specialist teams

### Reports and Export
- One-click fleet health report (plain text, suitable for shift handovers)
- Equipment registry export to CSV with health scores included
- Maintenance task export to CSV with priority and days-remaining columns
- All exports saved under an `Exports/` folder beside the executable

---

## Tech Stack

| Layer | Technology |
|---|---|
| UI framework | WPF (.NET 8, Windows) |
| Language | C# 12 |
| Data storage | JSON via System.Text.Json |
| Architecture | MVVM-lite — INotifyPropertyChanged, ObservableCollection |
| Unit testing | xUnit 2.9 with coverlet |
| Build tooling | .NET SDK 8 |

---

## Project Structure

```
C# project/
├── AeroMaintain/
│   ├── Models/
│   │   ├── Equipment.cs               # Core entity; NextDueDate derived from interval
│   │   ├── EquipmentStatus.cs         # Healthy / Watch / Critical enum
│   │   ├── MaintenanceTask.cs         # Flat read model used by the Scheduler tab
│   │   ├── HealthAssessment.cs        # Score + label + summary from HealthScoreService
│   │   └── TroubleshootingRule.cs     # Symptom -> causes + recommended checks
│   ├── Services/
│   │   ├── HealthScoreService.cs      # Penalty-based scoring (0-100)
│   │   ├── MaintenanceService.cs      # Task builder and filter engine
│   │   ├── TroubleshootingService.cs  # Case-insensitive symptom lookup
│   │   ├── DataService.cs             # Async JSON load/save; path injectable for tests
│   │   └── ExportService.cs           # CSV and plain-text report generation
│   ├── Data/
│   │   ├── equipment.json             # 10 pre-loaded industrial equipment records
│   │   └── troubleshooting_rules.json # 8 symptom rule entries
│   ├── MainWindow.xaml                # All UI: 5 tabs, styles, data bindings
│   └── MainWindow.xaml.cs             # Code-behind, ViewModel properties, handlers
└── AeroMaintain.Tests/
    ├── HealthScoreServiceTests.cs      # 9 tests
    ├── MaintenanceServiceTests.cs      # 8 tests
    ├── TroubleshootingServiceTests.cs  # 4 tests
    ├── DataServiceTests.cs             # 5 tests
    └── ExportServiceTests.cs           # 11 tests  →  38 total
```

---

## Application Flow

```
Data/*.json
    |
    v
DataService  ──>  MainWindow (INotifyPropertyChanged + ObservableCollection)
                       |
                       +──> HealthScoreService      (per-unit score calculation)
                       +──> MaintenanceService      (task building + filtering)
                       +──> TroubleshootingService  (symptom rule lookup)
                       +──> ExportService           (CSV + text report to Exports/)
```

`MainWindow.xaml` declares all bindings and tab layouts. `MainWindow.xaml.cs` coordinates user actions and drives view refreshes. Business logic lives entirely in `Services/` and is independently testable without a UI.

---

## Health Score Algorithm

Each unit starts at 100. Penalties are subtracted and the result is clamped to [0, 100]:

```
score = 100
      - min(45, overdueDays × 2)    // overdue days penalty, capped at 45
      - statusPenalty               // Healthy = 0, Watch = 15, Critical = 35
      - min(25, issueCount × 5)     // recent-issue penalty, capped at 25
```

| Score | Label |
|---|---|
| 80 – 100 | Stable |
| 55 – 79 | Needs Attention |
| 0 – 54 | Immediate Check Required |

---

## Prerequisites

- Windows 10 or Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

Verify your installation:

```powershell
dotnet --info
```

---

## Build and Run

```powershell
# From the repository root
dotnet restore
dotnet run --project AeroMaintain

# Release build
dotnet build AeroMaintain -c Release
# Output: AeroMaintain\bin\Release\net8.0-windows\AeroMaintain.exe
```

The app opens with 10 pre-loaded equipment records spread across Healthy, Watch, and Critical states so every dashboard feature is immediately visible.

---

## Running Tests

```powershell
# Run all 38 unit tests
dotnet test AeroMaintain.Tests

# With detailed output
dotnet test AeroMaintain.Tests --logger "console;verbosity=normal"
```

| Test class | Count | Coverage focus |
|---|---|---|
| HealthScoreServiceTests | 9 | Scoring, penalty caps, label thresholds, fleet averages |
| MaintenanceServiceTests | 8 | Due date calculation, priority labels, all filter combinations |
| TroubleshootingServiceTests | 4 | Case-insensitive matching, null on miss, multi-rule selection |
| DataServiceTests | 5 | Save/load round-trip, field fidelity, overwrite, missing-file guard |
| ExportServiceTests | 11 | File creation, CSV headers, data rows, comma escaping, report content |

---

## Manual Smoke Test

1. Open the dashboard — confirm all six summary cards populate with values.
2. Use the **Equipment Registry** search to filter by name and by category.
3. Select a record, click **Mark Maintained (Today)** — verify the next-due date advances.
4. Add a new equipment item, save, restart the app, confirm it reloads.
5. Switch to the **Maintenance Scheduler**, cycle through every status and due-date filter.
6. Open the **Guided Troubleshooting** tab and cycle through all 8 symptoms.
7. Go to **Reports and Export**, generate the text report, and export both CSV files.
8. Open the `Exports/` folder beside the executable and inspect the generated files.

---

## Possible Next Steps

- Move user-modified data from the build output folder to `%LOCALAPPDATA%\AeroMaintain`.
- Add a maintenance history log instead of storing only the latest service date.
- Full MVVM with `ICommand`-based view models and a dependency injection container.
- A CI workflow (GitHub Actions) that runs restore → Release build → test on every push.
- A versioned Windows installer published from the Actions release pipeline.

---

## Scope

AeroMaintain is a portfolio and learning project. Its troubleshooting output is rule-based demonstration guidance, not a substitute for approved maintenance procedures or certified engineering judgement.
