using System.Globalization;
using Template.Communication.Requests;
using Template.Communication.Responses;
using Template.Domain.Repositories;
using Template.Exception;

namespace Template.Application.Common;

internal static class Pagination
{
    public static List<string> Validate(RequestPaginationJson request)
    {
        var errors = new List<string>();

        if (request.Pagina < 1)
            errors.Add(ResourcesMessageException.INVALID_PAGE);

        if (request.TamanhoPagina is < 1 or > RequestPaginationJson.MaxPageSize)
            errors.Add(string.Format(CultureInfo.CurrentCulture, ResourcesMessageException.INVALID_PAGE_SIZE, RequestPaginationJson.MaxPageSize));

        return errors;
    }

    public static ResponsePagedJson<TResponse> ToResponse<TEntity, TResponse>(
        PagedResult<TEntity> page,
        RequestPaginationJson request,
        Func<TEntity, TResponse> map) => new()
    {
        Itens = page.Items.Select(map).ToList(),
        Pagina = request.Pagina,
        TamanhoPagina = request.TamanhoPagina,
        TotalItens = page.TotalCount,
        TotalPaginas = (int)Math.Ceiling(page.TotalCount / (double)request.TamanhoPagina)
    };
}
