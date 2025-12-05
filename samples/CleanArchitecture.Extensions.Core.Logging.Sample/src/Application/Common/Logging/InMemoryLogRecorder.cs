using System.Collections.Concurrent;
using CleanArchitecture.Extensions.Core.Logging;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Logging;

public sealed class InMemoryLogRecorder : ILogRecorder
{
    private const int MaxEntries = 200;
    private readonly ConcurrentQueue<LogEntry> _entries = new();

    public void Record(LogEntry entry)
    {
        _entries.Enqueue(entry);

        while (_entries.Count > MaxEntries && _entries.TryDequeue(out _))
        {
            // Trim old entries to keep memory bounded.
        }
    }

    public IReadOnlyCollection<LogEntry> ReadRecent(int count = 50)
    {
        var items = _entries.ToArray();
        return items
            .Reverse()
            .Take(Math.Clamp(count, 1, MaxEntries))
            .ToArray();
    }
}
