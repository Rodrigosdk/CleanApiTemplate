using Microsoft.AspNetCore.HttpLogging;

namespace Template.Api.Logging;

public static class RequestLoggingSetup
{
    public static IServiceCollection AddRequestLogging(this IServiceCollection services)
    {
        // Uma linha por requisição (método, rota, status e duração). Sem corpo, query string ou
        // cabeçalhos, porque podem conter dados pessoais (LGPD).
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestMethod
                                    | HttpLoggingFields.RequestPath
                                    | HttpLoggingFields.ResponseStatusCode
                                    | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });

        return services;
    }
}
