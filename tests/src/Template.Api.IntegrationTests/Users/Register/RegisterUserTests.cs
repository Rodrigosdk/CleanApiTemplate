using System.Net;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using Template.Communication.Requests;
using Template.Domain.Validations;
using Template.Exception;
using Template.Infrastructure.Security.PasswordHashing;
using Shouldly;

namespace Template.Api.IntegrationTests.Users.Register;

public class RegisterUserTests(TemplateApplicationFactory factory) : UsersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_Cadastrar_Quando_RequisicaoForValida))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_Cadastrar_Quando_RequisicaoForValida()
    {
        var request = RequestUserJsonBuilder.Build();

        var response = await PostWithoutToken(RegisterUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var saved = await FindActiveByEmail(request.Email.ToLowerInvariant());
        saved.Active.ShouldBeTrue();
        saved.Name.ShouldBe(request.Nome.Trim());
        saved.Email.ShouldBe(request.Email.ToLowerInvariant());
        saved.Password.ShouldNotBe(request.Senha);
        new Argon2PasswordHasher().VerifyPassword(request.Senha, saved.Password).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_NaoExporASenha_Quando_Cadastrar))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_NaoExporASenha_Quando_Cadastrar()
    {
        var request = RequestUserJsonBuilder.Build();

        var response = await PostWithoutToken(RegisterUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain(request.Senha);
        body.ShouldNotContain("senha", Case.Insensitive);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemGravar_Quando_EmailJaEstiverEmUso))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_RetornarBadRequestSemGravar_Quando_EmailJaEstiverEmUso()
    {
        var existing = RequestUserJsonBuilder.Build();
        (await PostWithoutToken(RegisterUri, existing)).StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await PostWithoutToken(RegisterUri, RequestUserJsonBuilder.Build(email: existing.Email));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        (await CountByEmail(existing.Email.ToLowerInvariant())).ShouldBe(1);
    }

    [Fact(DisplayName = nameof(Deve_CadastrarNovoUsuarioSemReativar_Quando_EmailForDeUsuarioInativo))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_CadastrarNovoUsuarioSemReativar_Quando_EmailForDeUsuarioInativo()
    {
        var original = RequestUserJsonBuilder.Build();
        var inactiveUserId = await RegisterAndDeactivate(original);

        var response = await PostWithoutToken(RegisterUri, RequestUserJsonBuilder.Build(email: original.Email, password: original.Senha));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var email = original.Email.ToLowerInvariant();
        var created = await FindActiveByEmail(email);
        created.Id.ShouldNotBe(inactiveUserId);
        var inactive = await FindById(inactiveUserId);
        inactive.Active.ShouldBeFalse();
        inactive.DeactivatedAt.ShouldNotBeNull();
        (await CountByEmail(email)).ShouldBe(2);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestComTodosOsErros_Quando_CamposForemInvalidos))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_RetornarBadRequestComTodosOsErros_Quando_CamposForemInvalidos()
    {
        var request = RequestUserJsonBuilder.Build(name: "   ", email: "invalido", password: "curta");

        var response = await PostWithoutToken(RegisterUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(ResourcesMessageException.USER_NAME_REQUIRED);
        body.ShouldContain(ResourcesMessageException.INVALID_EMAIL);
        body.ShouldContain(FieldLimits.UserPasswordTooShort);
        (await CountByEmail("invalido")).ShouldBe(0);
    }

    private async Task<Guid> RegisterAndDeactivate(RequestUserJson request)
    {
        var response = await PostWithoutToken(RegisterUri, request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var email = request.Email.ToLowerInvariant();
        var user = await DbContext.Users.SingleAsync(item => item.Email == email);
        user.Deactivate();
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        return user.Id;
    }
}
