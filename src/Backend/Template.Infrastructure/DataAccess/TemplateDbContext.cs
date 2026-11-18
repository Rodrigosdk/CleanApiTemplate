using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;
using Template.Infrastructure.DataAccess.Mappings;

[assembly: InternalsVisibleTo("Template.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("Template.Api.IntegrationTests")]

namespace Template.Infrastructure.DataAccess;

internal class TemplateDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserMap());
    }
}
