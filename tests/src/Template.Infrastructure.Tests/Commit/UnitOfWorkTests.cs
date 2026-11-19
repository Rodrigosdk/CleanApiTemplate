using CommonTestUtilities.Entities;
using Microsoft.EntityFrameworkCore;
using Template.Exception;
using Template.Exception.Exception;
using Template.Infrastructure.DataAccess;
using Shouldly;

namespace Template.Infrastructure.Tests.Commit;

public class UnitOfWorkTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact(DisplayName = nameof(Deve_PersistirAlteracoes_Quando_ConfirmarUnitOfWork))]
    [Trait("Infrastructure", "UnitOfWork - Commit")]
    public async Task Deve_PersistirAlteracoes_Quando_ConfirmarUnitOfWork()
    {
        var userId = await SaveUser();
        await using var dbContext = fixture.CreateDbContext();
        var tracked = await dbContext.Users.SingleAsync(user => user.Id == userId);
        tracked.Deactivate();

        await new UnitOfWork(dbContext).CommitAsync();

        await using var verificationContext = fixture.CreateDbContext();
        var saved = await verificationContext.Users.IgnoreQueryFilters().SingleAsync(user => user.Id == userId);
        saved.Active.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_LancarConflitoSemSobrescrever_Quando_RegistroForAlteradoPorOutraOperacao))]
    [Trait("Infrastructure", "UnitOfWork - Concorrência")]
    public async Task Deve_LancarConflitoSemSobrescrever_Quando_RegistroForAlteradoPorOutraOperacao()
    {
        var userId = await SaveUser();
        await using var firstRequest = fixture.CreateDbContext();
        await using var secondRequest = fixture.CreateDbContext();
        var userInFirst = await firstRequest.Users.SingleAsync(user => user.Id == userId);
        var userInSecond = await secondRequest.Users.SingleAsync(user => user.Id == userId);

        userInFirst.Update("Primeira Alteração", userInFirst.Email, userInFirst.Password);
        await new UnitOfWork(firstRequest).CommitAsync();

        userInSecond.Update("Segunda Alteração", userInSecond.Email, userInSecond.Password);
        var exception = await Should.ThrowAsync<ConflictException>(() => new UnitOfWork(secondRequest).CommitAsync());

        exception.Message.ShouldBe(ResourcesMessageException.CONCURRENCY_CONFLICT);
        await using var verificationContext = fixture.CreateDbContext();
        (await verificationContext.Users.AsNoTracking().SingleAsync(user => user.Id == userId))
            .Name.ShouldBe("Primeira Alteração");
    }

    [Fact(DisplayName = nameof(Deve_PermitirGravacoesSeguidas_Quando_UsarOMesmoContexto))]
    [Trait("Infrastructure", "UnitOfWork - Concorrência")]
    public async Task Deve_PermitirGravacoesSeguidas_Quando_UsarOMesmoContexto()
    {
        var userId = await SaveUser();
        await using var dbContext = fixture.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);
        var user = await dbContext.Users.SingleAsync(item => item.Id == userId);

        user.Update("Nome Intermediário", user.Email, user.Password);
        await unitOfWork.CommitAsync();
        user.Update("Nome Final", user.Email, user.Password);
        await unitOfWork.CommitAsync();

        await using var verificationContext = fixture.CreateDbContext();
        (await verificationContext.Users.AsNoTracking().SingleAsync(item => item.Id == userId))
            .Name.ShouldBe("Nome Final");
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_DoisUsuariosForemCadastradosComOMesmoEmailAoMesmoTempo))]
    [Trait("Infrastructure", "UnitOfWork - Concorrência")]
    public async Task Deve_LancarValidacao_Quando_DoisUsuariosForemCadastradosComOMesmoEmailAoMesmoTempo()
    {
        const string sharedEmail = "shared.email@example.com";
        await using var cleanContext = await fixture.CreateCleanDbContextAsync();
        await using var firstRequest = fixture.CreateDbContext();
        await using var secondRequest = fixture.CreateDbContext();
        firstRequest.Users.Add(UserBuilder.Build(email: sharedEmail));
        secondRequest.Users.Add(UserBuilder.Build(email: sharedEmail));

        await new UnitOfWork(firstRequest).CommitAsync();
        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => new UnitOfWork(secondRequest).CommitAsync());

        exception.GetErrorMessages().ShouldBe([ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS]);
        await using var verificationContext = fixture.CreateDbContext();
        (await verificationContext.Users.CountAsync(user => user.Email == sharedEmail)).ShouldBe(1);
    }

    private async Task<Guid> SaveUser()
    {
        await using var dbContext = fixture.CreateDbContext();
        var user = UserBuilder.Build();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user.Id;
    }
}
