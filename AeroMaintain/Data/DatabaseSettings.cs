using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AeroMaintain.Data;

public sealed class DatabaseSettings
{
    private const string ProviderEnvironmentVariable = "AEROMAINTAIN_DB_PROVIDER";
    private const string ConnectionStringEnvironmentVariable = "AEROMAINTAIN_CONNECTION_STRING";

    public DatabaseProvider Provider { get; init; }
    public string ConnectionString { get; init; } = string.Empty;
    public string Source { get; init; } = "Default";

    public bool IsSqlite => Provider == DatabaseProvider.Sqlite;
    public bool IsSqlServer => Provider == DatabaseProvider.SqlServer;

    public string ProviderLabel => Provider switch
    {
        DatabaseProvider.SqlServer => "SQL Server",
        _ => "SQLite"
    };

    public static DatabaseSettings ResolveDefault()
    {
        var appDataDirectory = ResolveAppDataDirectory();
        var settingsPath = Path.Combine(appDataDirectory, "database.settings.json");

        var environmentProvider = Environment.GetEnvironmentVariable(ProviderEnvironmentVariable);
        var environmentConnection = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentProvider) || !string.IsNullOrWhiteSpace(environmentConnection))
        {
            return FromValues(
                environmentProvider,
                environmentConnection,
                Path.Combine(appDataDirectory, "aeromaintain.db"),
                "Environment variables");
        }

        if (File.Exists(settingsPath))
        {
            var file = JsonSerializer.Deserialize<DatabaseSettingsFile>(File.ReadAllText(settingsPath));
            return FromValues(
                file?.Provider,
                file?.ConnectionString,
                string.IsNullOrWhiteSpace(file?.SqlitePath)
                    ? Path.Combine(appDataDirectory, "aeromaintain.db")
                    : file.SqlitePath,
                settingsPath);
        }

        return ForSqlitePath(Path.Combine(appDataDirectory, "aeromaintain.db"), "Default local SQLite");
    }

    public static DatabaseSettings ForSqlitePath(string dbPath, string source = "Injected SQLite path")
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Pooling = false
        }.ToString();

        return new DatabaseSettings
        {
            Provider = DatabaseProvider.Sqlite,
            ConnectionString = connectionString,
            Source = source
        };
    }

    public static DatabaseSettings ForSqlServer(string connectionString, string source = "Injected SQL Server connection")
    {
        return new DatabaseSettings
        {
            Provider = DatabaseProvider.SqlServer,
            ConnectionString = connectionString,
            Source = source
        };
    }

    public static string ResolveSettingsFilePath()
    {
        return Path.Combine(ResolveAppDataDirectory(), "database.settings.json");
    }

    public string Describe()
    {
        return $"{ProviderLabel} ({Source})";
    }

    private static DatabaseSettings FromValues(
        string? providerText,
        string? connectionString,
        string defaultSqlitePath,
        string source)
    {
        var provider = ParseProvider(providerText);
        if (provider == DatabaseProvider.SqlServer)
        {
            var sqlConnection = string.IsNullOrWhiteSpace(connectionString)
                ? "Server=.\\SQLEXPRESS;Database=AeroMaintain;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
                : connectionString.Trim();

            return ForSqlServer(sqlConnection, source);
        }

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return new DatabaseSettings
            {
                Provider = DatabaseProvider.Sqlite,
                ConnectionString = connectionString.Trim(),
                Source = source
            };
        }

        return ForSqlitePath(defaultSqlitePath, source);
    }

    private static DatabaseProvider ParseProvider(string? providerText)
    {
        return Enum.TryParse<DatabaseProvider>(providerText, ignoreCase: true, out var provider)
            ? provider
            : DatabaseProvider.Sqlite;
    }

    private static string ResolveAppDataDirectory()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AeroMaintain");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private sealed class DatabaseSettingsFile
    {
        public string? Provider { get; set; }
        public string? ConnectionString { get; set; }
        public string? SqlitePath { get; set; }
    }
}
