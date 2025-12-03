using CleanArchitecture.Extensions.Core.Logging;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Extensions.Core.Pipeline.Sample.Application.Common.Logging;

public sealed class MelLogContext : ILogContext
{
    private readonly ILogger _logger;

    public MelLogContext(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("LogContext");
    }

    public string? CorrelationId { get; set; }

    public IDisposable PushProperty(string name, object? value)
    {
        return _logger.BeginScope(new Dictionary<string, object?> { [name] = value })!;
    }
}
