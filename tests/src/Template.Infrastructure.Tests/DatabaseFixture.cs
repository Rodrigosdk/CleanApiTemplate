using CommonTestUtilities.Database;
using Microsoft.EntityFrameworkCore;
using Template.Infrastructure.DataAccess;

namespace Template.Infrastructure.Tests;

public sealed class DatabaseFixture : IAsyncLifetime, IAsyncDisposable
{
    private const string CleanTablesSql = "TRUNCATE TABLE users CASCADE;";

    private readonly TestDatabase _database = new("template_infrastructure_tests");

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _database.DisposeAsync();

    ValueTask IAsyncDisposable.DisposeAsync() => _database.DisposeAsync();

    internal TemplateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TemplateDbContext>();
        options.UseDatabase(_database.ConnectionString);

        return new TemplateDbContext(options.Options);
    }

    internal async Task<TemplateDbContext> CreateCleanDbContextAsync()
    {
        var dbContext = CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync(CleanTablesSql);

        return dbContext;
    }
}
