# AeroMaintain App

WPF desktop app for equipment registry management, maintenance scheduling, operational notifications, role-based access control, maintenance completion history, audit logging, troubleshooting guidance, shared database configuration, and operational exports.

## Runtime Data

- SQLite database: stored in the user's LocalAppData `AeroMaintain` folder by default.
- SQL Server mode: enabled with `%LocalAppData%\AeroMaintain\database.settings.json` or `AEROMAINTAIN_DB_PROVIDER` / `AEROMAINTAIN_CONNECTION_STRING`.
- EF Core migrations: applied automatically on startup via `Database.Migrate()`.
- Legacy local databases: older databases created with `EnsureCreated()` are baselined before production logging migrations run.
- JSON files in `Data/`: used as first-run seed data for demo equipment and troubleshooting rules.

## Main Areas

- `Data/AeroMaintainDbContext.cs`: EF Core model configuration.
- `Data/DatabaseSettings.cs`: SQLite/SQL Server provider resolution.
- `Data/database.settings.example.json`: SQL Server Express example configuration.
- `Migrations/`: baseline equipment schema plus maintenance/audit logging migration.
- `Models/Equipment.cs`: persisted equipment entity.
- `Models/MaintenanceLog.cs`: completed maintenance history.
- `Models/MaintenanceAlert.cs`: active operational notification read model.
- `Models/AuditLog.cs`: create/update/delete audit records.
- `Models/UserRole.cs` and `Models/UserPermission.cs`: role and permission primitives.
- `Services/DataService.cs`: migrations, seed loading, equipment persistence, maintenance logging, and audit writes.
- `Services/PermissionService.cs`: role-permission matrix.
- `Services/NotificationService.cs`: builds critical, warning, and info alerts from maintenance tasks.
- `Services/DesktopNotificationService.cs`: sends Windows tray-style alert summaries.
- `MainWindow.xaml`: dashboard, registry, scheduler, troubleshooting, history/audit, and export tabs.

## Local Run

```powershell
dotnet run --project AeroMaintain
```

## Notes

The app is still local/single-user, but the persistence foundation now supports future production work such as role-based authentication, SQL Server, email or Teams alerts, analytics, and supervisor-managed troubleshooting rules.
