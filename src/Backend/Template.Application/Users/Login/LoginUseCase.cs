#if (logging)
using Microsoft.Extensions.Logging;
using Template.Application.Logging;
#endif
using Template.Communication.Requests;
using Template.Communication.Responses;
using Template.Domain.Repositories.Users;
using Template.Domain.Security.PasswordHashing;
using Template.Domain.Security.Tokens;
using Template.Domain.Validations;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Application.Users.Login;

internal class LoginUseCase(
    IUsersReadOnlyRepository usersReadOnlyRepository,
    IPasswordHashing passwordHashing,
#if (logging)
    IAccessTokenGenerator accessTokenGenerator,
    ILogger<LoginUseCase> logger) : ILoginUseCase
#else
    IAccessTokenGenerator accessTokenGenerator) : ILoginUseCase
#endif
{
    public async Task<ResponseLoginJson> ExecuteAsync(RequestLoginJson request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var user = await usersReadOnlyRepository.GetByEmailAsync(EmailValidator.Normalize(request.Email), cancellationToken);
        if (user is null)
        {
            passwordHashing.HashPassword(request.Senha);
            throw InvalidCredentials();
        }

        if (!passwordHashing.VerifyPassword(request.Senha, user.Password))
        {
            throw InvalidCredentials();
        }

        var accessToken = accessTokenGenerator.Generate(user.Id);
#if (logging)
        logger.UserLoggedIn(user.Id);
#endif

        return new ResponseLoginJson
        {
            Nome = user.Name,
            Token = accessToken.Token,
            ExpiraEm = accessToken.ExpiresAt
        };
    }

#if (logging)
    private UnauthorizedException InvalidCredentials()
    {
        logger.UserLoginFailed();
#else
    private static UnauthorizedException InvalidCredentials()
    {
#endif
        return new UnauthorizedException(ResourcesMessageException.USER_INVALID_CREDENTIALS);
    }

    private static void Validate(RequestLoginJson request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(ResourcesMessageException.INVALID_EMAIL);

        if (string.IsNullOrWhiteSpace(request.Senha))
            errors.Add(ResourcesMessageException.USER_PASSWORD_REQUIRED);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
