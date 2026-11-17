using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("Template.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("Template.Api.IntegrationTests")]

namespace Template.Infrastructure.DataAccess;

internal class TemplateDbContext(DbContextOptions options) : DbContext(options)
{
}
