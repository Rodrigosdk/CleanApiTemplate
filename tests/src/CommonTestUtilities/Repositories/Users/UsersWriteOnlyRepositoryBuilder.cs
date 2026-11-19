using Moq;
using Template.Domain.Entities;
using Template.Domain.Repositories.Users;

namespace CommonTestUtilities.Repositories.Users;

public static class UsersWriteOnlyRepositoryBuilder
{
    public static IUsersWriteOnlyRepository Build()
    {
        var mock = new Mock<IUsersWriteOnlyRepository>();
        mock.Setup(repository => repository.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        return mock.Object;
    }
}

