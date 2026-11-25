using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
#if (auth)
using Template.Domain.Entities;
using Template.Infrastructure.DataAccess.Mappings;
#endif

[assembly: InternalsVisibleTo("Template.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("Template.Api.IntegrationTests")]

namespace Template.Infrastructure.DataAccess;

internal class TemplateDbContext(DbContextOptions options) : DbContext(options)
{
#if (mysql)
    public const string VersionProperty = "Version";

#endif
#if (auth)
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserMap());
    }
#endif
#if (mysql)

    // O MySQL não tem um token de concorrência gerado pelo banco (como xmin ou rowversion):
    // a versão é renovada aqui a cada alteração e o EF a compara no UPDATE
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RenewVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RenewVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void RenewVersions()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added or EntityState.Modified && entry.Metadata.FindProperty(VersionProperty) is not null)
                entry.Property(VersionProperty).CurrentValue = Guid.NewGuid();
        }
    }
#endif
}
