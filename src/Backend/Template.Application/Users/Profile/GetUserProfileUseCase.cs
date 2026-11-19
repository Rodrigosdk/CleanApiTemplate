using Template.Communication.Responses;
using Template.Domain.Repositories.Users;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Application.Users.Profile;

internal class GetUserProfileUseCase(IUsersReadOnlyRepository usersReadOnlyRepository) : IGetUserProfileUseCase
{
    public async Task<ResponseUserProfileJson> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await usersReadOnlyRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(ResourcesMessageException.USER_NOT_FOUND);

        return new ResponseUserProfileJson { Nome = user.Name, Email = user.Email };
    }
}
