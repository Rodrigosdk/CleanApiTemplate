using System.Globalization;
using Template.Exception;

namespace Template.Domain.Validations;

public static class FieldLimits
{
    public const int Email = 254;
    public const int UserName = 150;
    public const int UserPasswordMinLength = 8;

    public static bool Exceeds(string? value, int maxLength) =>
        value is not null && value.Trim().Length > maxLength;

    public static string UserNameTooLong =>
        string.Format(CultureInfo.CurrentCulture, ResourcesMessageException.USER_NAME_TOO_LONG, UserName);

    public static string UserPasswordTooShort =>
        string.Format(CultureInfo.CurrentCulture, ResourcesMessageException.USER_PASSWORD_TOO_SHORT, UserPasswordMinLength);
}
