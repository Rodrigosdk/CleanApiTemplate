using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using Template.Application.Users.Register;
using Template.Communication.Requests;
using Template.Domain.Validations;
using Template.Exception;
using Template.Exception.Exception;
using Shouldly;
using Xunit.Sdk;

namespace Template.Application.Tests.Users.Register;

public class RegisterUserUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarUsuario_Quando_RequisicaoForValida))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RegistrarUsuario_Quando_RequisicaoForValida()
    {
        var request = RequestUserJsonBuilder.Build();
        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(request);

        result.ShouldNotBeNull();
        result.Nome.ShouldBe(request.Nome);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_JaExistirUsuarioAtivoComMesmoEmail))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacao_Quando_JaExistirUsuarioAtivoComMesmoEmail()
    {
        var request = RequestUserJsonBuilder.Build();
        var useCase = CreateUseCase(emailInUse: true);

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        });
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacao_Quando_NomeForVazio))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacao_Quando_NomeForVazio(string name)
    {
        await ShouldFailWithError(
            RequestUserJsonBuilder.Build(name: name),
            ResourcesMessageException.USER_NAME_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_NomeExcederOLimite))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacao_Quando_NomeExcederOLimite()
    {
        await ShouldFailWithError(
            RequestUserJsonBuilder.Build(name: new string('a', FieldLimits.UserName + 1)),
            FieldLimits.UserNameTooLong);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacao_Quando_EmailForInvalido))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("john")]
    [InlineData("john@example")]
    public async Task Deve_LancarValidacao_Quando_EmailForInvalido(string email)
    {
        await ShouldFailWithError(
            RequestUserJsonBuilder.Build(email: email),
            ResourcesMessageException.INVALID_EMAIL);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacao_Quando_SenhaForVazia))]
    [Trait("Application", "Users - Register")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacao_Quando_SenhaForVazia(string password)
    {
        await ShouldFailWithError(
            RequestUserJsonBuilder.Build(password: password),
            ResourcesMessageException.USER_PASSWORD_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_SenhaForMenorQueOMinimo))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_LancarValidacao_Quando_SenhaForMenorQueOMinimo()
    {
        await ShouldFailWithError(
            RequestUserJsonBuilder.Build(password: new string('a', FieldLimits.UserPasswordMinLength - 1)),
            FieldLimits.UserPasswordTooShort);
    }

    [Fact(DisplayName = nameof(Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos))]
    [Trait("Application", "Users - Register")]
    public async Task Deve_RetornarTodosOsErros_Quando_VariosCamposForemInvalidos()
    {
        var request = RequestUserJsonBuilder.Build(name: "", email: "invalido", password: "");
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(3);
            errorMessage.ShouldContain(ResourcesMessageException.USER_NAME_REQUIRED);
            errorMessage.ShouldContain(ResourcesMessageException.INVALID_EMAIL);
            errorMessage.ShouldContain(ResourcesMessageException.USER_PASSWORD_REQUIRED);
        });
    }

    private static async Task ShouldFailWithError(RequestUserJson request, string expectedError)
    {
        var useCase = CreateUseCase();

        var exception = await useCase.ExecuteAsync(request).ShouldThrowAsync<ErrorOnValidationException>();
        exception.GetErrorMessages().ShouldSatisfyAllConditions(errorMessage =>
        {
            errorMessage.Count.ShouldBe(1);
            errorMessage.ShouldContain(expectedError);
        });
    }

    private static RegisterUserUseCase CreateUseCase(bool emailInUse = false)
    {
        var usersReadOnlyRepositoryBuilder = new UsersReadOnlyRepositoryBuilder();

        if (emailInUse)
            usersReadOnlyRepositoryBuilder.ExistActiveWithEmail();

        return new RegisterUserUseCase(
            UsersWriteOnlyRepositoryBuilder.Build(),
            usersReadOnlyRepositoryBuilder.Build(),
            new PasswordHashingBuilder().Build(),
            UnitOfWorkBuilder.Build());
    }
}
