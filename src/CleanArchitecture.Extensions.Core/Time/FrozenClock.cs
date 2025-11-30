namespace CleanArchitecture.Extensions.Core.Time;

/// <summary>
/// Test-friendly clock that advances only when instructed.
/// </summary>
public sealed class FrozenClock : IClock
{
    private DateTimeOffset _current;
    private long _timestamp;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrozenClock"/> class.
    /// </summary>
    /// <param name="fixedTime">Optional starting time; defaults to <see cref="DateTimeOffset.UtcNow"/>.</param>
    public FrozenClock(DateTimeOffset? fixedTime = null)
    {
        _current = fixedTime ?? DateTimeOffset.UtcNow;
    }

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _current;

    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(_current.UtcDateTime);

    /// <inheritdoc />
    public long Timestamp => _timestamp;

    /// <inheritdoc />
    public Guid NewGuid() => Guid.NewGuid();

    /// <inheritdoc />
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        Advance(delay);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Advances the clock by the specified duration.
    /// </summary>
    /// <param name="duration">Duration to advance.</param>
    public void Advance(TimeSpan duration)
    {
        _current = _current.Add(duration);
        _timestamp += duration.Ticks;
    }
}
