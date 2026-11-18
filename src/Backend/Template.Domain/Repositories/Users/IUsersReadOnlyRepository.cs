using Template.Domain.Entities;

namespace Template.Domain.Repositories.Users;

public interface IUsersReadOnlyRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistActiveUserWithEmailAsync(string email, Guid? exceptUserId = null, CancellationToken cancellationToken = default);
}
