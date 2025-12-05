using CleanArchitecture.Extensions.Core.Logging;

namespace CleanArchitecture.Extensions.Core.Logging.Sample.Application.Common.Logging;

public interface ILogRecorder
{
    void Record(LogEntry entry);

    IReadOnlyCollection<LogEntry> ReadRecent(int count = 50);
}
