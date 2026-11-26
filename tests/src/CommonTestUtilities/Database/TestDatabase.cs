#if (postgres)
using Npgsql;
using Testcontainers.PostgreSql;
#elif (sqlserver)
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
#else
using MySql.Data.MySqlClient;
using Testcontainers.MySql;
#endif

namespace CommonTestUtilities.Database;

// Um único servidor por processo de teste, criado na primeira fixture; cada fixture recebe
// um banco próprio nele. O Testcontainers remove o container quando o processo termina.
public sealed class TestDatabase(string databaseName)
{
    private static readonly SemaphoreSlim StartLock = new(1, 1);
#if (postgres)
    private static PostgreSqlContainer? _server;
#elif (sqlserver)
    private static MsSqlContainer? _server;
#else
    private static MySqlContainer? _server;
#endif

    private readonly string _databaseName = $"{databaseName}_{Guid.NewGuid():N}"[..(databaseName.Length + 9)];

    public string ConnectionString =>
#if (postgres)
        new NpgsqlConnectionStringBuilder(Server.GetConnectionString()) { Database = _databaseName }.ConnectionString;
#elif (sqlserver)
        new SqlConnectionStringBuilder(Server.GetConnectionString()) { InitialCatalog = _databaseName }.ConnectionString;
#else
        new MySqlConnectionStringBuilder(Server.GetConnectionString()) { Database = _databaseName }.ConnectionString;
#endif

#if (postgres)
    private static PostgreSqlContainer Server =>
#elif (sqlserver)
    private static MsSqlContainer Server =>
#else
    private static MySqlContainer Server =>
#endif
        _server ?? throw new InvalidOperationException("Chame StartServerAsync antes de usar o banco de testes.");

    public static async Task StartServerAsync()
    {
        await StartLock.WaitAsync();
        try
        {
            if (_server is not null)
                return;

#if (postgres)
            var server = new PostgreSqlBuilder("postgres:17-alpine")
                .WithUsername("template")
                .WithPassword("test-password")
                .Build();
#elif (sqlserver)
            var server = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Test-Password-123")
                .Build();
#else
            var server = new MySqlBuilder("mysql:8.4")
                .WithUsername("root")
                .WithPassword("test-password")
                .Build();
#endif
            await server.StartAsync();
            _server = server;
        }
        finally
        {
            StartLock.Release();
        }
    }
}
