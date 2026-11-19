using Template.Communication.Requests;
using Template.Communication.Responses;

namespace Template.Application.Users.Register;

public interface IRegisterUserUseCase
{
    public Task<ResponseRegisteredUserJson> ExecuteAsync(RequestUserJson request, CancellationToken cancellationToken = default);
}
