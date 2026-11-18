using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Template.Domain.Repositories;
using Template.Domain.Repositories.Users;
using Template.Domain.Security.PasswordHashing;
using Template.Domain.Security.Tokens;
using Template.Exception;
using Template.Infrastructure.Configuration;
using Template.Infrastructure.DataAccess;
using Template.Infrastructure.DataAccess.Repositories;
using Template.Infrastructure.Security.PasswordHashing;
using Template.Infrastructure.Security.Tokens;

namespace Template.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);
        AddDatabase(services, configuration);
        AddRepositories(services);
        AddSecurity(services, configuration);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUsersWriteOnlyRepository, UsersRepository>();
        services.AddScoped<IUsersReadOnlyRepository, UsersRepository>();
        services.AddScoped<IUsersUpdateOnlyRepository, UsersRepository>();
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
    
    private static void AddSecurity(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHashing, Argon2PasswordHasher>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => options.IsValid(), ResourcesMessageException.INVALID_JWT_CONFIGURATION)
            .ValidateOnStart();

        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
    }
}
