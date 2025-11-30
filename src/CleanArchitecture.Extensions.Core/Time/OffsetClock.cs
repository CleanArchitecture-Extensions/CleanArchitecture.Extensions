namespace CleanArchitecture.Extensions.Core.Time;

/// <summary>
/// Clock that applies a fixed offset to an underlying clock instance.
/// </summary>
public sealed class OffsetClock : IClock
{
    private readonly IClock _innerClock;
    private readonly TimeSpan _offset;

    /// <summary>
    /// Initializes a new instance of the <see cref="OffsetClock"/> class.
    /// </summary>
    /// <param name="innerClock">Underlying clock to offset.</param>
    /// <param name="offset">Offset to apply.</param>
    public OffsetClock(IClock innerClock, TimeSpan offset)
    {
        _innerClock = innerClock ?? throw new ArgumentNullException(nameof(innerClock));
        _offset = offset;
    }

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _innerClock.UtcNow.Add(_offset);

    /// <inheritdoc />
    public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);

    /// <inheritdoc />
    public long Timestamp => _innerClock.Timestamp;

    /// <inheritdoc />
    public Guid NewGuid() => _innerClock.NewGuid();

    /// <inheritdoc />
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default) => _innerClock.Delay(delay, cancellationToken);
}
