using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Users.Login;
using Template.Application.Users.Register;
using Template.Communication.Requests;
using Template.Communication.Responses;

namespace Template.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/autenticacao")]
public class AuthenticationController : ControllerBase
{
    [HttpPost("registrar")]
    [ProducesResponseType<ResponseRegisteredUserJson>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, ProblemJson.ContentType)]
    public async Task<IActionResult> Register(
        [FromBody] RequestUserJson request,
        [FromServices] IRegisterUserUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("entrar")]
    [ProducesResponseType<ResponseLoginJson>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, ProblemJson.ContentType)]
    public async Task<ActionResult<ResponseLoginJson>> Login(
        [FromBody] RequestLoginJson request,
        [FromServices] ILoginUseCase useCase,
        CancellationToken cancellationToken)
    {
        return Ok(await useCase.ExecuteAsync(request, cancellationToken));
    }
}
