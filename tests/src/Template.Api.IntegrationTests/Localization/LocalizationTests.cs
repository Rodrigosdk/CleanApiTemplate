using System.Net;
using Template.Api.IntegrationTests.Support;
using Shouldly;

namespace Template.Api.IntegrationTests.Localization;

public class LocalizationTests(TemplateApplicationFactory factory) : IClassFixture<TemplateApplicationFactory>
{
    [Theory(DisplayName = nameof(Deve_ResponderNoIdiomaPedido_Quando_AcceptLanguageForSuportado))]
    [Trait("Integration", "Localização")]
    [InlineData("en", "Invalid email.")]
    [InlineData("en-US", "Invalid email.")]
    [InlineData("pt-BR", "E-mail inválido.")]
    public async Task Deve_ResponderNoIdiomaPedido_Quando_AcceptLanguageForSuportado(string language, string expectedMessage)
    {
        var body = await SendInvalidRequest(language);

        body.ShouldContain(expectedMessage);
    }

    [Theory(DisplayName = nameof(Deve_ResponderEmPortugues_Quando_AcceptLanguageNaoForSuportadoOuAusente))]
    [Trait("Integration", "Localização")]
    [InlineData(null)]
    [InlineData("fr")]
    public async Task Deve_ResponderEmPortugues_Quando_AcceptLanguageNaoForSuportadoOuAusente(string? language)
    {
        var body = await SendInvalidRequest(language);

        body.ShouldContain("E-mail inválido.");
    }

    [Fact(DisplayName = nameof(Deve_FormatarMensagemComLimiteEmIngles_Quando_AcceptLanguageForIngles))]
    [Trait("Integration", "Localização")]
    public async Task Deve_FormatarMensagemComLimiteEmIngles_Quando_AcceptLanguageForIngles()
    {
        var body = await SendInvalidRequest("en");

        body.ShouldContain("The page size must be between 1 and 100.");
    }

    private async Task<string> SendInvalidRequest(string? language)
    {
        using var client = factory.CreateClient();
        if (language is not null)
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);

        var response = await client.GetAsync(TestController.ValidationUri);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        return await response.Content.ReadAsStringAsync();
    }
}
