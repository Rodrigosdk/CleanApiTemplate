using System.Globalization;
#if (auth)
using System.Net.Http.Headers;
#endif
using CommonTestUtilities.Database;
#if (auth)
using CommonTestUtilities.Security;
#endif
using CommonTestUtilities.Time;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
#if (auth)
using Microsoft.Extensions.Options;
#endif
using Microsoft.Extensions.Time.Testing;
using Template.Api.IntegrationTests.Support;
#if (auth)
using Template.Infrastructure.Configuration;
using Template.Infrastructure.Security.Tokens;
#endif

namespace Template.Api.IntegrationTests;

public class TemplateApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    static TemplateApplicationFactory()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
    }

    private readonly TestDatabase _database = new("template_integration_tests");

    public FakeTimeProvider Clock { get; } = TimeProviderBuilder.Build();
#if (auth)

    public JwtOptions Jwt { get; } = JwtOptionsBuilder.Build();

    public string CreateAccessToken(Guid? userId = null, DateTimeOffset? issuedAt = null)
    {
        return new JwtAccessTokenGenerator(Options.Create(Jwt), TimeProviderBuilder.Build(issuedAt ?? Clock.GetUtcNow())).Generate(userId ?? Guid.NewGuid()).Token;
    }

    public HttpClient Authorize(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAccessToken());
        return client;
    }

    public HttpClient CreateAnonymousClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        Authorize(client);
    }
#endif

    public async Task InitializeAsync() => await _database.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbConnection"] = _database.ConnectionString
            });
#if (auth)
            configuration.AddInMemoryCollection(JwtOptionsBuilder.Settings(Jwt));
#endif
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            services.AddControllers().AddApplicationPart(typeof(TestController).Assembly);
        });
    }
}
