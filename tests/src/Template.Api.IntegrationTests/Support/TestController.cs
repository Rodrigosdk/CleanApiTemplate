using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Template.Communication.Requests;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Api.IntegrationTests.Support;

// Endpoints que existem só nos testes, para exercitar o pipeline de erros e de localização
// sem depender dos recursos de negócio do projeto
[ApiController]
[Route("api/teste")]
[SuppressMessage("Performance", "CA1822", Justification = "Ações de controller não podem ser estáticas")]
public class TestController : ControllerBase
{
    public const string ThrowUri = "/api/teste/erro";
    public const string ValidationUri = "/api/teste/validacao";
    public const string BodyUri = "/api/teste/corpo";

    [HttpGet("erro")]
    public IActionResult Throw([FromServices] ExceptionToThrow exceptionToThrow) => throw exceptionToThrow.Exception;

    [HttpGet("validacao")]
    public IActionResult Validation() => throw new ErrorOnValidationException(
    [
        ResourcesMessageException.INVALID_EMAIL,
        string.Format(CultureInfo.CurrentCulture, ResourcesMessageException.INVALID_PAGE_SIZE, RequestPaginationJson.MaxPageSize)
    ]);

    [HttpPost("corpo")]
    public IActionResult Body([FromBody] TestBody body) => Ok(body);
}

public sealed class ExceptionToThrow(System.Exception exception)
{
    public System.Exception Exception { get; } = exception;
}

public sealed class TestBody
{
    public int Quantidade { get; set; }
}
