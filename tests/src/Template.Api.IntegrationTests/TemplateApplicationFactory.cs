using System.Globalization;
using System.Net.Http.Headers;
using CommonTestUtilities.Database;
using CommonTestUtilities.Security;
using CommonTestUtilities.Time;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Template.Api.IntegrationTests.Support;
using Template.Infrastructure.Configuration;
using Template.Infrastructure.Security.Tokens;

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
            configuration.AddInMemoryCollection(JwtOptionsBuilder.Settings(Jwt));
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            services.AddControllers().AddApplicationPart(typeof(TestController).Assembly);
        });
    }
}
