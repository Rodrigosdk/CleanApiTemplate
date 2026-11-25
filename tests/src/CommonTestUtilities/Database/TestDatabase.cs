#if (sqlserver)
using Microsoft.Data.SqlClient;
#endif
#if (postgres)
using Testcontainers.PostgreSql;
#else
using Testcontainers.MsSql;
#endif

namespace CommonTestUtilities.Database;

public sealed class TestDatabase(string databaseName) : IAsyncDisposable
{
#if (postgres)
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase(databaseName)
        .WithUsername("template")
        .WithPassword("test-password")
        .Build();
#else
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test-Password-123")
        .Build();
#endif

#if (sqlserver)
    public string ConnectionString =>
        new SqlConnectionStringBuilder(_container.GetConnectionString()) { InitialCatalog = databaseName }.ConnectionString;
#else
    public string ConnectionString => _container.GetConnectionString();
#endif

    public Task StartAsync() => _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
