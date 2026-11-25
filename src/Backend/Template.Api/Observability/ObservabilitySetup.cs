#if (postgres)
using Npgsql;
#endif
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Template.Api.Observability;

public static class ObservabilitySetup
{
    // Variável padrão do OpenTelemetry; sem ela, nada é exportado (testes e execução local sem coletor)
    private const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
#if (postgres)
                .AddNpgsql()
#elif (sqlserver)
                .AddSqlClientInstrumentation()
#else
                .AddSource("connector-net")
#endif
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
#if (postgres)
                .AddMeter("Npgsql")
#endif
                .AddMeter("Microsoft.EntityFrameworkCore"));

        if (!string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointKey]))
            openTelemetry.UseOtlpExporter();

        return builder;
    }
}
