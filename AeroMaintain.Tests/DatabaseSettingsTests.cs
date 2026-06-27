using AeroMaintain.Data;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class DatabaseSettingsTests
{
    [Fact]
    public void ForSqlitePath_BuildsSqliteConnectionWithPoolingDisabled()
    {
        var settings = DatabaseSettings.ForSqlitePath(@"C:\Temp\aeromaintain-test.db", "test");

        Assert.Equal(DatabaseProvider.Sqlite, settings.Provider);
        Assert.True(settings.IsSqlite);
        Assert.False(settings.IsSqlServer);
        Assert.Contains("Data Source=C:\\Temp\\aeromaintain-test.db", settings.ConnectionString);
        Assert.Contains("Pooling=False", settings.ConnectionString);
        Assert.Equal("SQLite (test)", settings.Describe());
    }

    [Fact]
    public void ForSqlServer_PreservesConnectionStringAndProvider()
    {
        const string connectionString = "Server=.\\SQLEXPRESS;Database=AeroMaintain;Trusted_Connection=True;TrustServerCertificate=True";

        var settings = DatabaseSettings.ForSqlServer(connectionString, "test sql");

        Assert.Equal(DatabaseProvider.SqlServer, settings.Provider);
        Assert.False(settings.IsSqlite);
        Assert.True(settings.IsSqlServer);
        Assert.Equal(connectionString, settings.ConnectionString);
        Assert.Equal("SQL Server (test sql)", settings.Describe());
    }

    [Fact]
    public void DataService_ExposesInjectedDatabaseSettings()
    {
        var settings = DatabaseSettings.ForSqlitePath(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db"), "injected");
        using var cleanup = new TemporaryDatabaseFile(settings.ConnectionString);

        var service = new DataService(settings, seedFromJson: false);

        Assert.Same(settings, service.Settings);
    }

    private sealed class TemporaryDatabaseFile : IDisposable
    {
        private readonly string _path;

        public TemporaryDatabaseFile(string connectionString)
        {
            var prefix = "Data Source=";
            var start = connectionString.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            _path = start < 0
                ? string.Empty
                : connectionString[(start + prefix.Length)..].Split(';')[0];
        }

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(_path) && File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
    }
}
