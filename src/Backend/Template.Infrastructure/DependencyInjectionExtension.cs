using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Template.Domain.Repositories;
using Template.Exception;
using Template.Infrastructure.Configuration;
using Template.Infrastructure.DataAccess;

namespace Template.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);
        AddDatabase(services, configuration);
    }

    
    private static void AddDatabase(IServiceCollection services,  IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.DbConnection),
                ResourcesMessageException.MISSING_CONNECTION_STRING)
            .ValidateOnStart();

        services.AddDbContext<TemplateDbContext>((serviceProvider, config) =>
            config.UseDatabase(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.DbConnection));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHealthChecks().AddDbContextCheck<TemplateDbContext>("database");
    }
    
}
