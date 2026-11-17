namespace Template.Domain.Validations;

public static class FieldLimits
{
    public const int Email = 254;

    public static bool Exceeds(string? value, int maxLength) =>
        value is not null && value.Trim().Length > maxLength;
}
