using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Template.Infrastructure.DataAccess;

// Usada só pelas ferramentas do EF (dotnet ef migrations add / script), nunca pela API.
// Gerar migrations e scripts não conecta ao banco, então uma connection string de referência basta;
// para comandos que conectam (database update), defina ConnectionStrings__DbConnection.
internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
{
#if (postgres)
    private const string ReferenceConnectionString = "Host=localhost;Port=5432;Database=Template";
#elif (sqlserver)
    private const string ReferenceConnectionString = "Server=localhost,1433;Database=Template;TrustServerCertificate=True";
#else
    private const string ReferenceConnectionString = "Server=localhost;Port=3306;Database=Template";
#endif

    public TemplateDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DbConnection");

        var options = new DbContextOptionsBuilder<TemplateDbContext>();
        options.UseDatabase(string.IsNullOrWhiteSpace(connectionString) ? ReferenceConnectionString : connectionString);

        return new TemplateDbContext(options.Options);
    }
}
