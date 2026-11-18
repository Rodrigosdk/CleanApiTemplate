using Template.Application.Common;
using Template.Communication.Requests;
using Template.Domain.Repositories;
using Template.Exception;
using Shouldly;

namespace Template.Application.Tests.Common;

public class PaginationTests
{
    [Fact(DisplayName = nameof(Deve_NaoRetornarErros_Quando_PaginacaoForValida))]
    [Trait("Application", "Paginação - Validação")]
    public void Deve_NaoRetornarErros_Quando_PaginacaoForValida()
    {
        var errors = Pagination.Validate(new RequestPaginationJson { Pagina = 1, TamanhoPagina = RequestPaginationJson.MaxPageSize });

        errors.ShouldBeEmpty();
    }

    [Theory(DisplayName = nameof(Deve_RetornarErro_Quando_PaginaForMenorQueUm))]
    [Trait("Application", "Paginação - Validação")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deve_RetornarErro_Quando_PaginaForMenorQueUm(int page)
    {
        var errors = Pagination.Validate(new RequestPaginationJson { Pagina = page });

        errors.ShouldBe([ResourcesMessageException.INVALID_PAGE]);
    }

    [Theory(DisplayName = nameof(Deve_RetornarErro_Quando_TamanhoDaPaginaEstiverForaDoLimite))]
    [Trait("Application", "Paginação - Validação")]
    [InlineData(0)]
    [InlineData(RequestPaginationJson.MaxPageSize + 1)]
    public void Deve_RetornarErro_Quando_TamanhoDaPaginaEstiverForaDoLimite(int pageSize)
    {
        var errors = Pagination.Validate(new RequestPaginationJson { TamanhoPagina = pageSize });

        errors.ShouldHaveSingleItem().ShouldContain(RequestPaginationJson.MaxPageSize.ToString(System.Globalization.CultureInfo.CurrentCulture));
    }

    [Fact(DisplayName = nameof(Deve_CalcularTotalDePaginas_Quando_ConverterParaResposta))]
    [Trait("Application", "Paginação - Resposta")]
    public void Deve_CalcularTotalDePaginas_Quando_ConverterParaResposta()
    {
        var page = new PagedResult<int>([1, 2], 41);
        var request = new RequestPaginationJson { Pagina = 2, TamanhoPagina = 20 };

        var response = Pagination.ToResponse(page, request, item => item * 10);

        response.Itens.ShouldBe([10, 20]);
        response.Pagina.ShouldBe(2);
        response.TamanhoPagina.ShouldBe(20);
        response.TotalItens.ShouldBe(41);
        response.TotalPaginas.ShouldBe(3);
    }
}
