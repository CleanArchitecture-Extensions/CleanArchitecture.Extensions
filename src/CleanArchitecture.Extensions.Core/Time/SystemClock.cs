using System.Diagnostics;

namespace CleanArchitecture.Extensions.Core.Time;

/// <summary>
/// Default clock implementation that delegates to system time APIs.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <inheritdoc />
    public long Timestamp => Stopwatch.GetTimestamp();

    /// <inheritdoc />
    public Guid NewGuid() => Guid.NewGuid();

    /// <inheritdoc />
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default) => Task.Delay(delay, cancellationToken);
}
