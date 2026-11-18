using Template.Exception;
using Template.Exception.Exception;
using Shouldly;

namespace Template.Domain.Tests.Exceptions;

public class ExceptionMessageTests
{
    [Fact(DisplayName = nameof(Deve_ReunirTodosOsErrosNaMensagem_Quando_ValidacaoTiverVariosErros))]
    [Trait("Domain", "Exceções - ErrorOnValidationException")]
    public void Deve_ReunirTodosOsErrosNaMensagem_Quando_ValidacaoTiverVariosErros()
    {
        List<string> errors = [ResourcesMessageException.INVALID_REQUEST, ResourcesMessageException.INVALID_EMAIL];

        var exception = new ErrorOnValidationException(errors);

        exception.Message.ShouldBe($"{ResourcesMessageException.INVALID_REQUEST}; {ResourcesMessageException.INVALID_EMAIL}");
        exception.GetErrorMessages().ShouldBe(errors);
    }

    [Fact(DisplayName = nameof(Deve_UsarOErroComoMensagem_Quando_ValidacaoTiverUmErro))]
    [Trait("Domain", "Exceções - ErrorOnValidationException")]
    public void Deve_UsarOErroComoMensagem_Quando_ValidacaoTiverUmErro()
    {
        var exception = new ErrorOnValidationException(ResourcesMessageException.INVALID_EMAIL);

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_EMAIL);
        exception.GetErrorMessages().ShouldBe([ResourcesMessageException.INVALID_EMAIL]);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_RecursoNaoForEncontrado))]
    [Trait("Domain", "Exceções - NotFoundException")]
    public void Deve_ExporAMensagem_Quando_RecursoNaoForEncontrado()
    {
        var exception = new NotFoundException(ResourcesMessageException.UNKNOWN_ERROR);

        exception.Message.ShouldBe(ResourcesMessageException.UNKNOWN_ERROR);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_HouverConflito))]
    [Trait("Domain", "Exceções - ConflictException")]
    public void Deve_ExporAMensagem_Quando_HouverConflito()
    {
        var exception = new ConflictException(ResourcesMessageException.CONCURRENCY_CONFLICT);

        exception.Message.ShouldBe(ResourcesMessageException.CONCURRENCY_CONFLICT);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_RegraDeDominioForViolada))]
    [Trait("Domain", "Exceções - DomainRuleException")]
    public void Deve_ExporAMensagem_Quando_RegraDeDominioForViolada()
    {
        var exception = new DomainRuleException(ResourcesMessageException.INVALID_REQUEST);

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_REQUEST);
    }

    [Fact(DisplayName = nameof(Deve_ExporAMensagem_Quando_AcessoNaoForAutorizado))]
    [Trait("Domain", "Exceções - UnauthorizedException")]
    public void Deve_ExporAMensagem_Quando_AcessoNaoForAutorizado()
    {
        var exception = new UnauthorizedException(ResourcesMessageException.INVALID_REQUEST);

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_REQUEST);
    }
}
