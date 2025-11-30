namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// In-memory logger useful for tests and verification of emitted log entries.
/// </summary>
/// <typeparam name="T">Type used for log category naming.</typeparam>
public sealed class InMemoryAppLogger<T> : IAppLogger<T>
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _gate = new();
    private readonly ILogContext _context;
    private readonly Func<DateTimeOffset> _timestampProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryAppLogger{T}"/> class.
    /// </summary>
    /// <param name="context">Log context for correlation and properties.</param>
    /// <param name="timestampProvider">Provider for timestamps, defaulting to <see cref="DateTimeOffset.UtcNow"/>.</param>
    public InMemoryAppLogger(ILogContext? context = null, Func<DateTimeOffset>? timestampProvider = null)
    {
        _context = context ?? new NoOpLogContext();
        _timestampProvider = timestampProvider ?? (() => DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Gets the captured log entries.
    /// </summary>
    public IReadOnlyCollection<LogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return _entries.ToList();
            }
        }
    }

    /// <summary>
    /// Records a log entry in memory.
    /// </summary>
    /// <param name="level">Severity level.</param>
    /// <param name="message">Log message.</param>
    /// <param name="exception">Optional associated exception.</param>
    /// <param name="properties">Optional structured properties.</param>
    public void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        var entry = new LogEntry(_timestampProvider(), level, message, _context.CorrelationId, exception, properties);
        lock (_gate)
        {
            _entries.Add(entry);
        }
    }
}
