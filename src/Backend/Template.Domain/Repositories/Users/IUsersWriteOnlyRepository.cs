using Template.Domain.Entities;

namespace Template.Domain.Repositories.Users;

public interface IUsersWriteOnlyRepository
{
    Task AddAsync(User user);
}
