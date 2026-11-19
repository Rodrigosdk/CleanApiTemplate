using Template.Domain.Validations;
using Shouldly;

namespace Template.Domain.Tests.Validations;

public class FieldLimitsTests
{
    [Theory(DisplayName = nameof(Deve_NaoExceder_Quando_ValorForNuloOuEstiverDentroDoLimite))]
    [Trait("Domain", "FieldLimits - Exceeds")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("aaaaa")]
    [InlineData("  aaaaa  ")]
    public void Deve_NaoExceder_Quando_ValorForNuloOuEstiverDentroDoLimite(string? value)
    {
        FieldLimits.Exceeds(value, 5).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_Exceder_Quando_ValorPassarDoLimite))]
    [Trait("Domain", "FieldLimits - Exceeds")]
    public void Deve_Exceder_Quando_ValorPassarDoLimite()
    {
        FieldLimits.Exceeds("aaaaaa", 5).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_InformarOLimiteNaMensagem_Quando_NomeDoUsuarioForLongoDemais))]
    [Trait("Domain", "FieldLimits - Mensagens")]
    public void Deve_InformarOLimiteNaMensagem_Quando_NomeDoUsuarioForLongoDemais()
    {
        FieldLimits.UserNameTooLong.ShouldContain(FieldLimits.UserName.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
