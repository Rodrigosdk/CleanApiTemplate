using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Template.Api.IntegrationTests.HealthChecks;

public class HealthCheckTests(TemplateApplicationFactory factory) : IClassFixture<TemplateApplicationFactory>
{
#if (postgres)
    private const string UnreachableDatabase =
        "Host=127.0.0.1;Port=1;Database=template;Username=template;Password=x;Timeout=2";
#else
    private const string UnreachableDatabase =
        "Server=127.0.0.1,1;Database=template;User Id=template;Password=x;Connect Timeout=2;TrustServerCertificate=True";
#endif

    [Fact(DisplayName = nameof(Deve_RetornarHealthy_Quando_BancoEstiverDisponivel))]
    [Trait("Integration", "Health check")]
    public async Task Deve_RetornarHealthy_Quando_BancoEstiverDisponivel()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldBe("Healthy");
    }

    [Fact(DisplayName = nameof(Deve_IniciarSemMigrarERetornarUnhealthy_Quando_ProducaoNaoAlcancarOBanco))]
    [Trait("Integration", "Health check")]
    public async Task Deve_IniciarSemMigrarERetornarUnhealthy_Quando_ProducaoNaoAlcancarOBanco()
    {
        var app = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DbConnection"] = UnreachableDatabase
                }));
        });

        var response = await app.CreateClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).ShouldBe("Unhealthy");
    }
}
