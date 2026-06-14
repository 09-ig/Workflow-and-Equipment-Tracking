# AeroMaintain

AeroMaintain is a Windows desktop application for tracking equipment condition,
maintenance deadlines, and first-pass troubleshooting guidance. It is a student
portfolio project built with C#, .NET 8, and WPF.

## What It Does

- Maintains an equipment registry backed by local JSON.
- Calculates each unit's next maintenance date.
- Highlights overdue, upcoming, watchlist, and critical work.
- Produces an explainable health score from 0 to 100.
- Looks up possible causes and checks for common symptoms.
- Exports equipment and maintenance data as CSV and text reports.

## Application Flow

```text
Data/*.json
    |
    v
DataService -> MainWindow -> Observable collections -> WPF controls
                  |
                  +-> MaintenanceService
                  +-> HealthScoreService
                  +-> TroubleshootingService
                  +-> ExportService -> Exports/
```

`MainWindow.xaml` defines the interface. `MainWindow.xaml.cs` coordinates user
actions and refreshes the displayed data. The reusable business rules live in
`Services/`, while the data structures live in `Models/`.

## Health Score

Every unit begins at 100 points. The app subtracts penalties for:

- overdue maintenance: 2 points per day, capped at 45;
- equipment status: 0 for Healthy, 15 for Watch, 35 for Critical;
- recent issues: 5 points per issue, capped at 25.

The result is clamped between 0 and 100 and mapped to Stable, Needs Attention,
or Immediate Check Required.

## Requirements

- Windows 10 or Windows 11
- .NET 8 SDK or a newer SDK capable of targeting .NET 8

Check your installation:

```powershell
dotnet --info
```

## Run Locally

From the repository root:

```powershell
dotnet restore
dotnet build "C# project.sln"
dotnet run --project .\AeroMaintain\AeroMaintain.csproj
```

The application should open with six sample equipment records. Try changing a
record, filtering the maintenance scheduler, selecting a troubleshooting
symptom, and generating an export.

## Verify the Project

Run the automated checks:

```powershell
dotnet test "C# project.sln"
```

Before publishing a release, also perform this short manual smoke test:

1. Open the dashboard and confirm all summary cards contain values.
2. Add, edit, and delete a temporary equipment record.
3. Restart the app and confirm the saved data reloads.
4. Test each scheduler filter.
5. Open every troubleshooting symptom.
6. Generate both CSV exports and the text report.

GitHub Actions repeats the restore, Release build, and test steps on every pull
request and push to `main`.

## Repository Structure

```text
.
|-- .github/workflows/ci.yml
|-- AeroMaintain/
|   |-- Data/
|   |-- Models/
|   |-- Services/
|   |-- MainWindow.xaml
|   `-- MainWindow.xaml.cs
|-- AeroMaintain.Tests/
|-- C# project.sln
`-- README.md
```

## Good Next Improvements

- Move user-modified data from the build folder to Windows local app data.
- Add friendly error handling for malformed JSON and failed file writes.
- Split `MainWindow` into smaller MVVM view models and commands.
- Add maintenance history instead of storing only the latest date.
- Add screenshots or a short demo GIF under `AeroMaintain/Assets/Screenshots/`.
- Publish a versioned Windows release from GitHub Actions.

## Scope

AeroMaintain is a portfolio and learning project. Its troubleshooting output is
demonstration guidance, not a substitute for approved maintenance procedures.
