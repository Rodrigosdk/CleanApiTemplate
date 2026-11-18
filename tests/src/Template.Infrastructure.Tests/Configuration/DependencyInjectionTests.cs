using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Template.Exception;
using Template.Infrastructure.DataAccess;
using Shouldly;

namespace Template.Infrastructure.Tests.Configuration;

public class DependencyInjectionTests
{
    private const string ValidConnectionString = "Server=localhost;Database=Template";

    [Theory(DisplayName = nameof(Deve_FalharNaInicializacaoComMensagemExplicativa_Quando_ConnectionStringNaoForConfigurada))]
    [Trait("Infrastructure", "Configuração - Connection string")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_FalharNaInicializacaoComMensagemExplicativa_Quando_ConnectionStringNaoForConfigurada(string? connectionString)
    {
        using var provider = BuildProvider(connectionString);

        var exception = Should.Throw<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        exception.Message.ShouldContain("ConnectionStrings:DbConnection");
        exception.Message.ShouldContain("ConnectionStrings__DbConnection");
    }

    [Fact(DisplayName = nameof(Deve_LancarMensagemExplicativa_Quando_DbContextForCriadoSemConnectionString))]
    [Trait("Infrastructure", "Configuração - Connection string")]
    public void Deve_LancarMensagemExplicativa_Quando_DbContextForCriadoSemConnectionString()
    {
        using var provider = BuildProvider(null);
        using var scope = provider.CreateScope();

        var exception = Should.Throw<OptionsValidationException>(
            () => scope.ServiceProvider.GetRequiredService<TemplateDbContext>());

        exception.Message.ShouldContain("ConnectionStrings__DbConnection");
    }

    [Fact(DisplayName = nameof(Deve_PassarNaValidacaoDeInicializacao_Quando_ConnectionStringForConfigurada))]
    [Trait("Infrastructure", "Configuração - Connection string")]
    public void Deve_PassarNaValidacaoDeInicializacao_Quando_ConnectionStringForConfigurada()
    {
        using var provider = BuildProvider(ValidConnectionString);

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Fact(DisplayName = nameof(Deve_ManterOTimeProviderRegistrado_Quando_ForSubstituidoAntesDaInfraestrutura))]
    [Trait("Infrastructure", "Configuração - Relógio")]
    public void Deve_ManterOTimeProviderRegistrado_Quando_ForSubstituidoAntesDaInfraestrutura()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 11, 1, 0, 0, TimeSpan.Zero));
        using var provider = BuildProvider(ValidConnectionString, configureServices: services => services.AddSingleton<TimeProvider>(timeProvider));

        provider.GetRequiredService<TimeProvider>().ShouldBeSameAs(timeProvider);
    }

    [Fact(DisplayName = nameof(Deve_UsarORelogioDoSistema_Quando_NenhumTimeProviderForRegistrado))]
    [Trait("Infrastructure", "Configuração - Relógio")]
    public void Deve_UsarORelogioDoSistema_Quando_NenhumTimeProviderForRegistrado()
    {
        using var provider = BuildProvider(ValidConnectionString);

        provider.GetRequiredService<TimeProvider>().ShouldBeSameAs(TimeProvider.System);
    }


    private static ServiceProvider BuildProvider(
        string? connectionString,
        Action<IServiceCollection>? configureServices = null)
    {
        var settings = new Dictionary<string, string?>();
        settings["ConnectionStrings:DbConnection"] = connectionString;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }
}
