using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;
using Template.Domain.Repositories.Users;
using Template.Infrastructure.DataAccess.Mappings;

namespace Template.Infrastructure.DataAccess.Repositories;

internal sealed class UsersRepository(TemplateDbContext dbContext) :
    IUsersWriteOnlyRepository,
    IUsersReadOnlyRepository,
    IUsersUpdateOnlyRepository
{
    public Task AddAsync(User user)
    {
        dbContext.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<bool> ExistActiveUserWithEmailAsync(string email, Guid? exceptUserId = null, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(user =>
            user.Email == email &&
            (exceptUserId == null || user.Id != exceptUserId), cancellationToken);
    }

    public Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .IgnoreQueryFilters([UserMap.ActiveFilter])
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}
