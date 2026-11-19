using Bogus;
using CommonTestUtilities.Generators;
using Template.Domain.Entities;

namespace CommonTestUtilities.Entities;

public static class UserBuilder
{
    public static User Build(
        string? name = null,
        string? email = null,
        string? password = null)
    {
        return new Faker<User>()
            .CustomInstantiator(f => User.Create(
                name ?? f.Person.FullName,
                email ?? f.Internet.Email(f.Person.FirstName, f.Person.LastName).ToLowerInvariant(),
                password ?? f.Internet.Password(length: 12, prefix: "Aa1!")))
            .Generate();
    }

    public static User BuildToCreate(
        string? name = null,
        string? email = null,
        string? password = null)
    {
        var faker = new Faker();

        return User.Create(
            name ?? faker.Person.FullName,
            email ?? faker.Internet.Email(faker.Person.FirstName, faker.Person.LastName),
            password ?? PasswordGenerator.Generate());
    }
}
