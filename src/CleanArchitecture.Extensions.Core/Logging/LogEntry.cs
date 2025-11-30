namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// Represents a captured log entry with structured metadata.
/// </summary>
/// <param name="Timestamp">Time the log entry was captured.</param>
/// <param name="Level">Log severity level.</param>
/// <param name="Message">Log message.</param>
/// <param name="CorrelationId">Correlation identifier associated with the log entry.</param>
/// <param name="Exception">Optional exception for the log entry.</param>
/// <param name="Properties">Optional structured properties.</param>
public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Message,
    string? CorrelationId,
    Exception? Exception,
    IReadOnlyDictionary<string, object?>? Properties);
