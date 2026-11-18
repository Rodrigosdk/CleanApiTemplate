using Template.Domain.Entities;

namespace Template.Domain.Repositories.Users;

public interface IUsersUpdateOnlyRepository
{
    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default);
}
