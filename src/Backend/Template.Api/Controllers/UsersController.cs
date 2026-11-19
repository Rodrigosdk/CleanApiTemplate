using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Template.Application.Users.Profile;
using Template.Communication.Responses;

namespace Template.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsersController : ControllerBase
{
    [HttpGet("perfil")]
    [ProducesResponseType<ResponseUserProfileJson>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, ProblemJson.ContentType)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemJson.ContentType)]
    public async Task<ActionResult<ResponseUserProfileJson>> GetProfile(
        [FromServices] IGetUserProfileUseCase useCase,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

        return Ok(await useCase.ExecuteAsync(userId, cancellationToken));
    }
}
