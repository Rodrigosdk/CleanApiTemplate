using Template.Domain.SeedWorks;
using Template.Domain.Validations;
using Template.Exception;
using Template.Exception.Exception;

namespace Template.Domain.Entities;

public class User : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;
    public DateTime? DeactivatedAt { get; private set; }

    private User(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = password;
    }

    public static User Create(string name, string email, string password)
    {
        Validate(name, email, password);

        return new User(name.Trim(), EmailValidator.Normalize(email), password);
    }

    public void Update(string name, string email, string password)
    {
        Validate(name, email, password);

        Name = name.Trim();
        Email = EmailValidator.Normalize(email);
        Password = password;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Active = false;
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DeactivatedAt;
    }

    private static void Validate(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainRuleException(ResourcesMessageException.USER_NAME_REQUIRED);
        if (FieldLimits.Exceeds(name, FieldLimits.UserName))
            throw new DomainRuleException(FieldLimits.UserNameTooLong);
        if (!EmailValidator.IsValid(email))
            throw new DomainRuleException(ResourcesMessageException.INVALID_EMAIL);
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainRuleException(ResourcesMessageException.USER_PASSWORD_REQUIRED);
    }
}
