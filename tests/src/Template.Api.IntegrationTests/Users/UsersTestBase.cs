using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;

namespace Template.Api.IntegrationTests.Users;

public abstract class UsersTestBase(TemplateApplicationFactory factory) : BaseIntegrationTest(factory)
{
    protected const string RegisterUri = "/api/autenticacao/registrar";
    protected const string LoginUri = "/api/autenticacao/entrar";

    protected Task<User> FindById(Guid id) =>
        DbContext.Users.IgnoreQueryFilters().AsNoTracking().SingleAsync(user => user.Id == id);

    protected Task<User> FindActiveByEmail(string email) =>
        DbContext.Users.AsNoTracking().SingleAsync(user => user.Email == email);

    protected Task<int> CountByEmail(string email) =>
        DbContext.Users.IgnoreQueryFilters().CountAsync(user => user.Email == email);
}
