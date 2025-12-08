namespace CleanArchitecture.Extensions.Core.DomainEvents;

/// <summary>
/// Provides a deterministic source of time for domain events, overridable for tests.
/// </summary>
internal static class DomainEventTime
{
    private static Func<DateTimeOffset> _now = () => DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the current timestamp for domain events.
    /// </summary>
    public static DateTimeOffset Now => _now();

    /// <summary>
    /// Sets the time provider for domain events. Intended for application bootstrap or testing.
    /// </summary>
    /// <param name="provider">Provider returning the current timestamp.</param>
    public static void SetProvider(Func<DateTimeOffset> provider)
    {
        _now = provider ?? throw new ArgumentNullException(nameof(provider));
    }
}
