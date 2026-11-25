using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure.DataAccess;

public static class DatabaseProvider
{
    public static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string connectionString) =>
#if (postgres)
        builder.UseNpgsql(connectionString);
#elif (sqlserver)
        builder.UseSqlServer(connectionString);
#else
        builder.UseMySQL(connectionString);
#endif
}
