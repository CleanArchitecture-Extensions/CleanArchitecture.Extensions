namespace CleanArchitecture.Extensions.Core.Time;

/// <summary>
/// Abstraction for retrieving time and unique values in a deterministic manner.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets the current UTC date.
    /// </summary>
    DateOnly Today { get; }

    /// <summary>
    /// Gets a high-resolution timestamp suitable for performance measurements.
    /// </summary>
    long Timestamp { get; }

    /// <summary>
    /// Creates a new GUID.
    /// </summary>
    Guid NewGuid();

    /// <summary>
    /// Waits asynchronously for the specified duration.
    /// </summary>
    /// <param name="delay">Delay to wait for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes after the delay.</returns>
    Task Delay(TimeSpan delay, CancellationToken cancellationToken = default);
}
