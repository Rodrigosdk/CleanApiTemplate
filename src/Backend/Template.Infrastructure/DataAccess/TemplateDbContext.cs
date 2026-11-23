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
#if (auth)
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserMap());
    }
#endif
}
