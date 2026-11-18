namespace Template.Domain.Security.Tokens;

public interface IAccessTokenGenerator
{
    AccessToken Generate(Guid userId);
}
