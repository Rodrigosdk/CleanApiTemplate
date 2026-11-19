using Template.Communication.Requests;
using Template.Communication.Responses;

namespace Template.Application.Users.Login;

public interface ILoginUseCase
{
    public Task<ResponseLoginJson> ExecuteAsync(RequestLoginJson request, CancellationToken cancellationToken = default);
}
