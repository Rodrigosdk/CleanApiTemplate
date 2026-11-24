using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Localization;
using Template.Api.Converters;
using Template.Api.Errors;
using Template.Api.Filters;
#if (logging)
using Template.Api.Logging;
#endif
using Template.Api.Observability;
#if (auth)
using Template.Api.Security;
#endif
using Template.Application;
using Template.Infrastructure;
using Template.Infrastructure.DataAccess.Migrations;

var builder = WebApplication.CreateBuilder(args);

const long maxRequestBodySizeInBytes = 64 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = maxRequestBodySizeInBytes);

#if (observability)
builder.AddObservability();
#endif
#if (logging)
builder.Services.AddRequestLogging();
#endif
builder.Services.AddErrorHandling();

builder.Services.AddControllers(options => options.Filters.Add<ExceptionFilter>()).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new StringConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
#if (auth)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});
#else
builder.Services.AddOpenApi();
#endif

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
#if (auth)
builder.Services.AddJwtAuthentication();
#endif

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new("pt-BR"),
        new("en")
    };

    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Ordem: o cabeçalho de correlação e o log de requisições envolvem o tratamento de erros,
// para que o status final (inclusive 500) apareça no log e o X-Trace-Id vá em toda resposta
app.UseCorrelationIdHeader();
#if (logging)
app.UseHttpLogging();
#endif
app.UseErrorHandling();

app.UseRequestLocalization();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Template API"));
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription().AllowAnonymous();
}

app.UseHttpsRedirection();

#if (auth)
app.UseAuthentication();
app.UseAuthorization();
#endif

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

if (!app.Environment.IsProduction())
{
    await ExecuteMigrationsAsync();
}

app.Run();

async Task ExecuteMigrationsAsync()
{
    await using var scope = app.Services.CreateAsyncScope();
    
    await DatabaseMigration.ExecuteMigrationsAsync(scope.ServiceProvider);
}

public partial class Program { }
