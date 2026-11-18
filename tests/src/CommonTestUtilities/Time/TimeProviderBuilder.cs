using Microsoft.Extensions.Time.Testing;

namespace CommonTestUtilities.Time;

public static class TimeProviderBuilder
{
    public static readonly DateTimeOffset DefaultUtcNow = new(2026, 3, 10, 15, 0, 0, TimeSpan.Zero);

    public static FakeTimeProvider Build(DateTimeOffset? utcNow = null) => new(utcNow ?? DefaultUtcNow);
}
