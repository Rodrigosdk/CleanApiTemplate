using Template.Infrastructure.DataAccess;
using Template.Infrastructure.DataAccess.Repositories;

namespace Template.Infrastructure.Tests.Users;

public abstract class UsersRepositoryTestBase(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    protected DatabaseFixture Fixture { get; } = fixture;

    internal static UsersRepository CreateRepository(TemplateDbContext dbContext) => new(dbContext);
}
