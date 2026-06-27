using System.IO;
using System.Globalization;
using System.Text.Json;
using AeroMaintain.Data;
using AeroMaintain.Models;
using Microsoft.EntityFrameworkCore;

namespace AeroMaintain.Services;

public class DataService
{
    private const string BaselineMigrationId = "20260625120000_BaselineEquipmentSchema";
    private const string EfProductVersion = "8.0.0";

    private readonly DatabaseSettings _settings;
    private readonly bool _seedFromJson;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public DataService() : this(DatabaseSettings.ResolveDefault()) { }

    public DataService(string dbPath, bool seedFromJson = true)
        : this(DatabaseSettings.ForSqlitePath(dbPath), seedFromJson)
    {
    }

    public DataService(DatabaseSettings settings, bool seedFromJson = true)
    {
        _settings = settings;
        _seedFromJson = seedFromJson;
        InitializeDatabase();
    }

    public DatabaseSettings Settings => _settings;

    public async Task<List<Equipment>> LoadEquipmentAsync()
    {
        await using var ctx = CreateContext();
        var items = await ctx.Equipment.AsNoTracking().ToListAsync();
        if (items.Count == 0 && _seedFromJson)
        {
            await SeedEquipmentFromJsonAsync();
            await using var ctx2 = CreateContext();
            items = await ctx2.Equipment.AsNoTracking().ToListAsync();
        }
        return items;
    }

    public async Task SaveEquipmentAsync(
        IEnumerable<Equipment> equipment,
        string? actor = null,
        UserRole actorRole = UserRole.Admin)
    {
        var incoming = equipment.ToList();
        await using var ctx = CreateContext();
        await using var transaction = await ctx.Database.BeginTransactionAsync();

        var existing = await ctx.Equipment.ToListAsync();
        var auditLogs = new List<AuditLog>();
        var userName = NormalizeActor(actor);
        var roleName = actorRole.ToString();
        var changedAtUtc = DateTime.UtcNow;

        var toDelete = existing.Where(e => incoming.All(n => n.Id != e.Id)).ToList();
        foreach (var removed in toDelete)
        {
            auditLogs.Add(CreateAudit(
                nameof(Equipment),
                removed.Id,
                "Deleted",
                "*",
                DescribeEquipment(removed),
                null,
                $"Equipment deleted: {removed.Name}",
                userName,
                roleName,
                changedAtUtc));
        }

        ctx.Equipment.RemoveRange(toDelete);

        foreach (var item in incoming)
        {
            var tracked = existing.FirstOrDefault(e => e.Id == item.Id);
            if (tracked is null)
            {
                ctx.Equipment.Add(item);
                auditLogs.Add(CreateAudit(
                    nameof(Equipment),
                    item.Id,
                    "Created",
                    "*",
                    null,
                    DescribeEquipment(item),
                    $"Equipment created: {item.Name}",
                    userName,
                    roleName,
                    changedAtUtc));
            }
            else
            {
                AddEquipmentChangeAudits(auditLogs, tracked, item, userName, roleName, changedAtUtc);
                ctx.Entry(tracked).CurrentValues.SetValues(item);
            }
        }

        if (auditLogs.Count > 0)
        {
            ctx.AuditLogs.AddRange(auditLogs);
        }

        await ctx.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<List<TroubleshootingRule>> LoadTroubleshootingRulesAsync()
    {
        await using var ctx = CreateContext();
        var rules = await ctx.TroubleshootingRules.AsNoTracking().ToListAsync();
        if (rules.Count == 0 && _seedFromJson)
        {
            await SeedRulesFromJsonAsync();
            await using var ctx2 = CreateContext();
            rules = await ctx2.TroubleshootingRules.AsNoTracking().ToListAsync();
        }
        return rules;
    }

    public async Task<List<MaintenanceLog>> LoadMaintenanceLogsAsync(Guid? equipmentId = null, int take = 200)
    {
        await using var ctx = CreateContext();
        var query = ctx.MaintenanceLogs.AsNoTracking();

        if (equipmentId.HasValue)
        {
            query = query.Where(l => l.EquipmentId == equipmentId.Value);
        }

        return await query
            .OrderByDescending(l => l.CompletedOn)
            .ThenByDescending(l => l.LoggedAtUtc)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> LoadAuditLogsAsync(int take = 200)
    {
        await using var ctx = CreateContext();
        return await ctx.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.ChangedAtUtc)
            .Take(take)
            .ToListAsync();
    }

    public async Task<MaintenanceLog> RecordMaintenanceAsync(
        Guid equipmentId,
        string performedBy,
        string workSummary,
        string partsReplaced,
        decimal cost,
        double laborHours,
        DateTime completedOn,
        string notes,
        string? actor = null,
        UserRole actorRole = UserRole.Admin)
    {
        if (cost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cost), "Cost cannot be negative.");
        }

        if (laborHours < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(laborHours), "Labor hours cannot be negative.");
        }

        await using var ctx = CreateContext();
        await using var transaction = await ctx.Database.BeginTransactionAsync();

        var equipment = await ctx.Equipment.FirstOrDefaultAsync(e => e.Id == equipmentId);
        if (equipment is null)
        {
            throw new InvalidOperationException("Equipment was not found.");
        }

        var completedDate = completedOn.Date;
        var userName = NormalizeActor(actor ?? performedBy);
        var roleName = actorRole.ToString();
        var previousMaintenanceDate = equipment.LastMaintenanceDate;
        var changedAtUtc = DateTime.UtcNow;

        equipment.LastMaintenanceDate = completedDate;

        var log = new MaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            EquipmentName = equipment.Name,
            CompletedOn = completedDate,
            PerformedBy = NormalizeActor(performedBy),
            WorkSummary = NormalizeText(workSummary, "Routine maintenance completed."),
            PartsReplaced = NormalizeText(partsReplaced, "None"),
            Cost = cost,
            LaborHours = laborHours,
            Notes = notes.Trim(),
            LoggedAtUtc = changedAtUtc
        };

        ctx.MaintenanceLogs.Add(log);
        ctx.AuditLogs.Add(CreateAudit(
            nameof(MaintenanceLog),
            log.Id,
            "Created",
            "*",
            null,
            log.WorkSummary,
            $"Maintenance logged for {equipment.Name}.",
            userName,
            roleName,
            changedAtUtc));

        if (previousMaintenanceDate.Date != completedDate)
        {
            ctx.AuditLogs.Add(CreateAudit(
                nameof(Equipment),
                equipment.Id,
                "Updated",
                nameof(Equipment.LastMaintenanceDate),
                FormatValue(previousMaintenanceDate),
                FormatValue(completedDate),
                $"Maintenance date updated for {equipment.Name}.",
                userName,
                roleName,
                changedAtUtc));
        }

        await ctx.SaveChangesAsync();
        await transaction.CommitAsync();
        return log;
    }

    private async Task SeedEquipmentFromJsonAsync()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "equipment.json");
        if (!File.Exists(jsonPath)) return;

        await using var stream = File.OpenRead(jsonPath);
        var items = await JsonSerializer.DeserializeAsync<List<Equipment>>(stream, JsonOptions);
        if (items is null) return;

        await using var ctx = CreateContext();
        ctx.Equipment.AddRange(items);
        await ctx.SaveChangesAsync();
    }

    private async Task SeedRulesFromJsonAsync()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "troubleshooting_rules.json");
        if (!File.Exists(jsonPath)) return;

        await using var stream = File.OpenRead(jsonPath);
        var rules = await JsonSerializer.DeserializeAsync<List<TroubleshootingRule>>(stream, JsonOptions);
        if (rules is null) return;

        await using var ctx = CreateContext();
        ctx.TroubleshootingRules.AddRange(rules);
        await ctx.SaveChangesAsync();
    }

    private void InitializeDatabase()
    {
        using var ctx = CreateContext();
        BaselineLegacyDatabaseIfNeeded(ctx);
        ctx.Database.Migrate();
    }

    private static void BaselineLegacyDatabaseIfNeeded(AeroMaintainDbContext ctx)
    {
        if (!ctx.Database.IsSqlite())
        {
            return;
        }

        ctx.Database.OpenConnection();
        try
        {
            var hasMigrationHistory = HasTable(ctx, "__EFMigrationsHistory");
            var hasLegacyEquipmentTable = HasTable(ctx, "Equipment");

            if (hasMigrationHistory || !hasLegacyEquipmentTable)
            {
                return;
            }

            ExecuteNonQuery(
                ctx,
                """
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                """);

            ExecuteNonQuery(
                ctx,
                $"""
                INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ('{BaselineMigrationId}', '{EfProductVersion}');
                """);
        }
        finally
        {
            ctx.Database.CloseConnection();
        }
    }

    private static bool HasTable(AeroMaintainDbContext ctx, string tableName)
    {
        var connection = ctx.Database.GetDbConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result, CultureInfo.InvariantCulture) > 0;
    }

    private static void ExecuteNonQuery(AeroMaintainDbContext ctx, string sql)
    {
        var connection = ctx.Database.GetDbConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private AeroMaintainDbContext CreateContext() => new(_settings);

    private static void AddEquipmentChangeAudits(
        ICollection<AuditLog> auditLogs,
        Equipment current,
        Equipment incoming,
        string userName,
        string roleName,
        DateTime changedAtUtc)
    {
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.Name), current.Name, incoming.Name, userName, roleName, changedAtUtc);
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.SerialNumber), current.SerialNumber, incoming.SerialNumber, userName, roleName, changedAtUtc);
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.Category), current.Category, incoming.Category, userName, roleName, changedAtUtc);
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.LastMaintenanceDate), current.LastMaintenanceDate, incoming.LastMaintenanceDate, userName, roleName, changedAtUtc);
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.MaintenanceIntervalDays), current.MaintenanceIntervalDays, incoming.MaintenanceIntervalDays, userName, roleName, changedAtUtc);
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.Status), current.Status, incoming.Status, userName, roleName, changedAtUtc);
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.RecentIssueCount), current.RecentIssueCount, incoming.RecentIssueCount, userName, roleName, changedAtUtc);
        AddFieldChangeAudit(auditLogs, current, nameof(Equipment.Notes), current.Notes, incoming.Notes, userName, roleName, changedAtUtc);
    }

    private static void AddFieldChangeAudit<T>(
        ICollection<AuditLog> auditLogs,
        Equipment equipment,
        string fieldName,
        T oldValue,
        T newValue,
        string userName,
        string roleName,
        DateTime changedAtUtc)
    {
        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            return;
        }

        auditLogs.Add(CreateAudit(
            nameof(Equipment),
            equipment.Id,
            "Updated",
            fieldName,
            FormatValue(oldValue),
            FormatValue(newValue),
            $"Updated {equipment.Name}: {fieldName} changed.",
            userName,
            roleName,
            changedAtUtc));
    }

    private static AuditLog CreateAudit(
        string entityName,
        Guid? entityId,
        string action,
        string fieldName,
        string? oldValue,
        string? newValue,
        string description,
        string userName,
        string roleName,
        DateTime changedAtUtc)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            ChangedAtUtc = changedAtUtc,
            UserName = userName,
            UserRole = roleName,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description
        };
    }

    private static string NormalizeActor(string? actor)
    {
        var value = string.IsNullOrWhiteSpace(actor) ? Environment.UserName : actor.Trim();
        return string.IsNullOrWhiteSpace(value) ? "Unknown user" : value;
    }

    private static string NormalizeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string DescribeEquipment(Equipment equipment)
    {
        return $"{equipment.Name} ({equipment.SerialNumber}) - {equipment.Category} - {equipment.Status}";
    }

    private static string? FormatValue<T>(T value)
    {
        return value switch
        {
            null => null,
            DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal number => number.ToString("0.##", CultureInfo.InvariantCulture),
            double number => number.ToString("0.##", CultureInfo.InvariantCulture),
            float number => number.ToString("0.##", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}
