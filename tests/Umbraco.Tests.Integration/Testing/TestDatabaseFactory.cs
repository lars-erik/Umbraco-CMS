// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Persistence.SqlServer;

namespace Umbraco.Cms.Tests.Integration.Testing;

public static class TestDatabaseFactory
{
    private static readonly Lock s_lock = new();
    private static ITestDatabase? s_instance;

    /// <summary>
    ///     Gets or creates the shared <see cref="ITestDatabase"/> singleton.
    ///     All test infrastructure paths (UmbracoIntegrationTestBase, UmbracoIntegrationFixtureBase,
    ///     and TestDatabaseSwapper) must share a single instance to avoid competing for the same
    ///     physical databases.
    /// </summary>
    public static ITestDatabase GetOrCreate(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactory, ILoggerFactory loggerFactory)
    {
        lock (s_lock)
        {
            if (s_instance is not null)
            {
                return s_instance;
            }

            Directory.CreateDirectory(settings.FilesPath);
            s_instance = Create(settings, dbFactory, loggerFactory);
            return s_instance;
        }
    }

    /// <summary>
    ///     Creates a TestDatabase instance
    /// </summary>
    /// <remarks>
    ///     SQL Server setup requires configured master connection string &amp; privileges to create database.
    /// </remarks>
    /// <example>
    ///     <code>
    /// # SQL Server Environment variable setup
    /// $ export Tests__Database__DatabaseType="SqlServer"
    /// $ export Tests__Database__SQLServerMasterConnectionString="Server=localhost,1433; User Id=sa; Password=MySuperSecretPassword123!;"
    /// </code>
    /// </example>
    /// <example>
    ///     <code>
    /// # Docker cheat sheet
    /// $ docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=MySuperSecretPassword123!" -e 'MSSQL_PID=Developer' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu
    /// </code>
    /// </example>
    private static ITestDatabase Create(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactory, ILoggerFactory loggerFactory) =>
        settings.DatabaseType switch
        {
            TestDatabaseSettings.TestDatabaseType.Sqlite => new SqliteTestDatabase(settings, dbFactory, loggerFactory),
            TestDatabaseSettings.TestDatabaseType.SqlServer => CreateSqlServer(settings, loggerFactory, dbFactory),
            TestDatabaseSettings.TestDatabaseType.LocalDb => CreateLocalDb(settings, loggerFactory, dbFactory),
            _ => throw new ApplicationException("Unsupported test database provider")
        };

    private static ITestDatabase CreateLocalDb(TestDatabaseSettings settings, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
    {
        var localDb = new LocalDb();

        if (!localDb.IsAvailable)
        {
            throw new InvalidOperationException("LocalDB is not available.");
        }

        return new LocalDbTestDatabase(settings, loggerFactory, localDb, dbFactory.Create());
    }

    private static ITestDatabase CreateSqlServer(TestDatabaseSettings settings, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory) =>
        //new LoggingSnapshotableTestDatabase(
        new SqlServerTestDatabase(settings, loggerFactory, dbFactory.Create())
        //)
        ;
}

internal class LoggingSnapshotableTestDatabase : ISnapshotableTestDatabase
{
    private readonly ISnapshotableTestDatabase _innerDb;
    private readonly Stopwatch _stopwatch = new();

    public LoggingSnapshotableTestDatabase(ISnapshotableTestDatabase innerDb)
    {
        _innerDb = innerDb;
    }

    public TestDatabaseInformation AttachEmpty()
    {
        _stopwatch.Restart();
        var result = _innerDb.AttachEmpty();
        TestContext.Progress.WriteLine($"{_innerDb.GetType()} attached empty db {result.Name} in {_stopwatch.Elapsed}");
        return result;
    }

    public TestDatabaseInformation AttachSchema()
    {
        _stopwatch.Restart();
        var result = _innerDb.AttachSchema();
        TestContext.Progress.WriteLine($"{_innerDb.GetType()} attached schema db {result.Name} in {_stopwatch.Elapsed}");
        return result;
    }

    public void Detach(TestDatabaseInformation id)
    {
        _stopwatch.Restart();
        _innerDb.Detach(id);
        TestContext.Progress.WriteLine($"{_innerDb.GetType()} detached schema db {id.Name} in {_stopwatch.Elapsed}");
    }

    public bool HasSnapshot(string snapshotKey)
    {
        _stopwatch.Restart();
        var result = _innerDb.HasSnapshot(snapshotKey);
        TestContext.Progress.WriteLine($"{_innerDb.GetType()} {(result ? "found" : "did not find")} snapshot {snapshotKey} in {_stopwatch.Elapsed}");
        return result;
    }

    public void CreateSnapshot(string snapshotKey, TestDatabaseInformation sourceMeta)
    {
        _stopwatch.Restart();
        _innerDb.CreateSnapshot(snapshotKey, sourceMeta);
        TestContext.Progress.WriteLine($"{_innerDb.GetType()} created snapshot {snapshotKey} from {sourceMeta.Name} in {_stopwatch.Elapsed}");
    }

    public TestDatabaseInformation AttachFromSnapshot(string snapshotKey)
    {
        _stopwatch.Restart();
        var result = _innerDb.AttachFromSnapshot(snapshotKey);
        TestContext.Progress.WriteLine($"{_innerDb.GetType()} attached db {result.Name} from snapshot {snapshotKey} in {_stopwatch.Elapsed}");
        return result;
    }
}
