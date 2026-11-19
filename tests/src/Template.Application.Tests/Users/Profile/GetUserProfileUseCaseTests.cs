using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories.Users;
using Template.Application.Users.Profile;
using Template.Domain.Entities;
using Template.Exception;
using Template.Exception.Exception;
using Shouldly;

namespace Template.Application.Tests.Users.Profile;

public class GetUserProfileUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RetornarNomeEEmail_Quando_UsuarioExistir))]
    [Trait("Application", "Users - Perfil")]
    public async Task Deve_RetornarNomeEEmail_Quando_UsuarioExistir()
    {
        var user = UserBuilder.Build();

        var result = await CreateUseCase(user).ExecuteAsync(user.Id);

        result.Nome.ShouldBe(user.Name);
        result.Email.ShouldBe(user.Email);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_UsuarioNaoExistirOuEstiverInativo))]
    [Trait("Application", "Users - Perfil")]
    public async Task Deve_LancarNaoEncontrado_Quando_UsuarioNaoExistirOuEstiverInativo()
    {
        var exception = await Should.ThrowAsync<NotFoundException>(() => CreateUseCase().ExecuteAsync(Guid.NewGuid()));

        exception.Message.ShouldBe(ResourcesMessageException.USER_NOT_FOUND);
    }

    private static GetUserProfileUseCase CreateUseCase(User? user = null)
    {
        var readOnlyRepository = new UsersReadOnlyRepositoryBuilder();
        if (user is not null)
            readOnlyRepository.GetById(user);

        return new GetUserProfileUseCase(readOnlyRepository.Build());
    }
}
