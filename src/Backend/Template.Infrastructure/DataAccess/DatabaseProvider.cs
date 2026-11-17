using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure.DataAccess;

public static class DatabaseProvider
{
    public static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string connectionString) =>
        builder.UseNpgsql(connectionString);
}
