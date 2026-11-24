#if (logging)
using Microsoft.Extensions.Logging;
using Template.Application.Logging;
#endif
using Template.Communication.Requests;
using Template.Communication.Responses;
using Template.Domain.Entities;
using Template.Domain.Repositories;
using Template.Domain.Repositories.Users;
using Template.Domain.Security.PasswordHashing;
using Template.Domain.Validations;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Application.Users.Register;

internal class RegisterUserUseCase(
    IUsersWriteOnlyRepository usersWriteOnlyRepository,
    IUsersReadOnlyRepository usersReadOnlyRepository,
    IPasswordHashing passwordHashing,
#if (logging)
    IUnitOfWork unitOfWork,
    ILogger<RegisterUserUseCase> logger) : IRegisterUserUseCase
#else
    IUnitOfWork unitOfWork) : IRegisterUserUseCase
#endif
{
    public async Task<ResponseRegisteredUserJson> ExecuteAsync(RequestUserJson request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var email = EmailValidator.Normalize(request.Email);
        if (await usersReadOnlyRepository.ExistActiveUserWithEmailAsync(email, cancellationToken: cancellationToken))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        }

        var user = User.Create(request.Nome, email, passwordHashing.HashPassword(request.Senha));

        await usersWriteOnlyRepository.AddAsync(user);
        await unitOfWork.CommitAsync(cancellationToken);
#if (logging)
        logger.UserRegistered(user.Id);
#endif

        return new ResponseRegisteredUserJson { Nome = request.Nome};
    }

    private static void Validate(RequestUserJson request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Nome))
            errors.Add(ResourcesMessageException.USER_NAME_REQUIRED);
        else if (FieldLimits.Exceeds(request.Nome, FieldLimits.UserName))
            errors.Add(FieldLimits.UserNameTooLong);

        if (!EmailValidator.IsValid(request.Email))
            errors.Add(ResourcesMessageException.INVALID_EMAIL);

        if (string.IsNullOrWhiteSpace(request.Senha))
            errors.Add(ResourcesMessageException.USER_PASSWORD_REQUIRED);
        else if (request.Senha.Length < FieldLimits.UserPasswordMinLength)
            errors.Add(FieldLimits.UserPasswordTooShort);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
