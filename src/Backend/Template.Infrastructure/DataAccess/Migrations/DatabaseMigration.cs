using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Infrastructure.DataAccess.Migrations;

public static class DatabaseMigration
{
    public static async Task ExecuteMigrationsAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<TemplateDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}