// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer;

// ReSharper disable ConvertToUsingDeclaration
namespace Umbraco.Cms.Tests.Integration.Testing;

/// <remarks>
///     It's not meant to be pretty, rushed port of LocalDb.cs + LocalDbTestDatabase.cs
/// </remarks>
public class SqlServerTestDatabase : SqlServerBaseTestDatabase, ITestDatabase, ISnapshotableTestDatabase
{
    public const string DatabaseName = "UmbracoTests";
    private readonly TestDatabaseSettings _settings;
    private readonly ConcurrentDictionary<string, string> _snapshotPaths = new();
    private readonly ConcurrentBag<string> _snapshotRestoredDatabases = new();
    private int _snapshotCounter;

    public SqlServerTestDatabase(TestDatabaseSettings settings, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));

        _settings = settings;

        var counter = 0;

        var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
            .Select(x => TestDatabaseInformation.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", false, _settings.SQLServerMasterConnectionString));

        var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
            .Select(x => TestDatabaseInformation.CreateWithMasterConnectionString($"{DatabaseName}-{++counter}", true, _settings.SQLServerMasterConnectionString));

        _testDatabases = schema.Concat(empty).ToList();
    }

    protected override void Initialize()
    {
        _prepareQueue = new BlockingCollection<TestDatabaseInformation>();
        _readySchemaQueue = new BlockingCollection<TestDatabaseInformation>();
        _readyEmptyQueue = new BlockingCollection<TestDatabaseInformation>();

        foreach (var meta in _testDatabases)
        {
            CreateDatabase(meta);
            _prepareQueue.Add(meta);
        }

        for (var i = 0; i < _settings.PrepareThreadCount; i++)
        {
            var thread = new Thread(PrepareDatabase);
            thread.Start();
        }
    }

    private void CreateDatabase(TestDatabaseInformation meta)
    {
        Drop(meta);

        using (var connection = new SqlConnection(_settings.SQLServerMasterConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, $@"
                    CREATE DATABASE {LocalDb.QuotedName(meta.Name)};
                    ALTER DATABASE {LocalDb.QuotedName(meta.Name)} SET RECOVERY SIMPLE;
                ");
                command.ExecuteNonQuery();
            }
        }
    }

    private void Drop(TestDatabaseInformation meta) => DropByName(meta.Name);

    private void DropByName(string name)
    {
        using (var connection = new SqlConnection(_settings.SQLServerMasterConnectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                SetCommand(command, "select count(1) from sys.databases where name = @0", name);
                var records = (int)command.ExecuteScalar();
                if (records == 0)
                {
                    return;
                }

                var sql = $@"
                        ALTER DATABASE {LocalDb.QuotedName(name)}
                        SET SINGLE_USER
                        WITH ROLLBACK IMMEDIATE";
                SetCommand(command, sql);
                command.ExecuteNonQuery();

                SetCommand(command, $@"DROP DATABASE {LocalDb.QuotedName(name)}");
                command.ExecuteNonQuery();
            }
        }
    }

    #region ISnapshotableTestDatabase

    /// <inheritdoc />
    public bool HasSnapshot(string snapshotKey) => _snapshotPaths.ContainsKey(snapshotKey);

    /// <inheritdoc />
    public void CreateSnapshot(string snapshotKey, TestDatabaseInformation sourceMeta)
    {
        using var connection = new SqlConnection(_settings.SQLServerMasterConnectionString);
        connection.Open();

        // Get default data/log directories from the server
        var (defaultDataPath, defaultLogPath) = GetDefaultPaths(connection);

        // RESTORE DATABASE cannot use SQL parameters for identifiers or file paths.
        var escapedBackupPath = backupPath.Replace("'", "''");
        var dataFilePath = Path.Combine(defaultDataPath, $"{sourceMeta.Name}.mdf").Replace("'", "''");
        var logFilePath = Path.Combine(defaultLogPath, $"{sourceMeta.Name}_log.ldf").Replace("'", "''");

        var cloneDataFilePath = Path.Combine(_snapshotDir, $"{sourceMeta.Name}.mdf").Replace("'", "''");
        var cloneLogFilePath = Path.Combine(_snapshotDir, $"{sourceMeta.Name}_log.ldf").Replace("'", "''");

        using var cmd = connection.CreateCommand();

        // BACKUP DATABASE cannot use SQL parameters for database name or file path.
        // Names are internally generated (not user input), matching existing DDL patterns.
        //cmd.CommandText = $@"
        //    BACKUP DATABASE {LocalDb.QuotedName(sourceMeta.Name)}
        //    TO DISK = N'{backupPath.Replace("'", "''")}'
        //    WITH INIT"; // , COMPRESSION < not supported on express

        cmd.CommandText = $@"
            ALTER DATABASE [{sourceMeta.Name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            exec sp_detach_db N'{sourceMeta.Name}', @skipchecks = TRUE
        ";
        cmd.ExecuteNonQuery();

        File.Copy(dataFilePath, cloneDataFilePath, true);
        File.Copy(logFilePath, cloneLogFilePath, true);

        cmd.CommandText = $@"
            exec sp_attach_db N'{sourceMeta.Name}', N'{dataFilePath}', N'{logFilePath}';
            ALTER DATABASE [{sourceMeta.Name}] SET MULTI_USER;
        ";
        cmd.ExecuteNonQuery();

        _snapshotPaths[snapshotKey] = cloneDataFilePath;
    }

    /// <inheritdoc />
    public TestDatabaseInformation AttachFromSnapshot(string snapshotKey)
    {
        if (!_snapshotPaths.TryGetValue(snapshotKey, out var backupPath))
        {
            throw new InvalidOperationException($"No snapshot found with key '{snapshotKey}'.");
        }

        var meta = _readySchemaQueue.Take();
        var dbName = meta.Name;

        _snapshotRestoredDatabases.Add(dbName);

        // Drop if a database with this name already exists
        //DropByName(dbName);

        using var connection = new SqlConnection(_settings.SQLServerMasterConnectionString);
        connection.Open();

        // Get default data/log directories from the server
        var (defaultDataPath, defaultLogPath) = GetDefaultPaths(connection);

        var dataFilePath = Path.Combine(defaultDataPath, $"{dbName}.mdf").Replace("'", "''");
        var logFilePath = Path.Combine(defaultLogPath, $"{dbName}_log.ldf").Replace("'", "''");

        // RESTORE DATABASE cannot use SQL parameters for identifiers or file paths.
        var escapedBackupPath = backupPath.Replace("'", "''");
        var cloneDataFilePath = escapedBackupPath;
        var cloneLogFilePath = escapedBackupPath.Replace(".mdf", "_log.ldf");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = $@"
            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            exec sp_detach_db N'{dbName}', @skipchecks = TRUE
        ";
        cmd.ExecuteNonQuery();

        File.Copy(cloneDataFilePath, dataFilePath, true);
        File.Copy(cloneLogFilePath, logFilePath, true);

        cmd.CommandText = $@"
            exec sp_attach_db N'{dbName}', N'{dataFilePath}', N'{logFilePath}';
            ALTER DATABASE [{dbName}] SET MULTI_USER;
        ";
        cmd.ExecuteNonQuery();

        return meta;
    }

    private static (string DataLogical, string LogLogical) GetLogicalFileNames(
        SqlConnection connection, string backupPath)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"RESTORE FILELISTONLY FROM DISK = N'{backupPath.Replace("'", "''")}'";

        string dataName = null;
        string logName = null;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var type = reader["Type"].ToString();
            var logicalName = reader["LogicalName"].ToString();
            if (type == "D")
            {
                dataName = logicalName;
            }
            else if (type == "L")
            {
                logName = logicalName;
            }
        }

        return (dataName ?? throw new InvalidOperationException("No data file found in backup."),
                logName ?? throw new InvalidOperationException("No log file found in backup."));
    }

    private static (string DataPath, string LogPath) GetDefaultPaths(SqlConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS DataPath,
                   SERVERPROPERTY('InstanceDefaultLogPath') AS LogPath";

        using var reader = cmd.ExecuteReader();
        reader.Read();
        return (reader["DataPath"].ToString()!, reader["LogPath"].ToString()!);
    }

    #endregion

    public override void TearDown()
    {
        if (_prepareQueue == null)
        {
            return;
        }

        _prepareQueue.CompleteAdding();
        while (_prepareQueue.TryTake(out _))
        {
        }

        _readyEmptyQueue.CompleteAdding();
        while (_readyEmptyQueue.TryTake(out _))
        {
        }

        _readySchemaQueue.CompleteAdding();
        while (_readySchemaQueue.TryTake(out _))
        {
        }

        // Drop pool databases
        Parallel.ForEach(_testDatabases, Drop);

        // Drop snapshot-restored databases
        foreach (var name in _snapshotRestoredDatabases)
        {
            DropByName(name);
        }

        // Clean up snapshot files
        if (Directory.Exists(_snapshotDir))
        {
            try
            {
                Directory.Delete(_snapshotDir, recursive: true);
            }
            catch
            {
                // Best-effort cleanup
            }
        }
    }
}
