using CommonTestUtilities.Time;
using Moq;
using Template.Domain.Security.Tokens;

namespace CommonTestUtilities.Security;

public static class AccessTokenGeneratorBuilder
{
    public static readonly DateTime ExpiresAt = TimeProviderBuilder.DefaultUtcNow.UtcDateTime.AddHours(1);

    public static string TokenFor(Guid userId) => $"token-{userId}";

    public static IAccessTokenGenerator Build()
    {
        var mock = new Mock<IAccessTokenGenerator>();
        mock.Setup(generator => generator.Generate(It.IsAny<Guid>()))
            .Returns((Guid userId) => new AccessToken(TokenFor(userId), ExpiresAt));
        return mock.Object;
    }
}
