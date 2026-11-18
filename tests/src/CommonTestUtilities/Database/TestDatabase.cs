using Testcontainers.PostgreSql;

namespace CommonTestUtilities.Database;

public sealed class TestDatabase(string databaseName) : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase(databaseName)
        .WithUsername("template")
        .WithPassword("test-password")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task StartAsync() => _container.StartAsync();

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}
