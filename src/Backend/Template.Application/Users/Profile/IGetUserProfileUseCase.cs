using Template.Communication.Responses;

namespace Template.Application.Users.Profile;

public interface IGetUserProfileUseCase
{
    public Task<ResponseUserProfileJson> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default);
}
