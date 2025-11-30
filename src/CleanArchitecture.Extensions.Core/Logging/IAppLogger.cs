namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// Abstraction over logging systems used throughout the extensions.
/// </summary>
/// <typeparam name="T">Type used for log category naming.</typeparam>
public interface IAppLogger<T>
{
    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <param name="level">Severity level.</param>
    /// <param name="message">Log message.</param>
    /// <param name="exception">Optional associated exception.</param>
    /// <param name="properties">Optional structured properties.</param>
    void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null);

    /// <summary>
    /// Writes a trace log entry.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="properties">Optional structured properties.</param>
    void Trace(string message, IReadOnlyDictionary<string, object?>? properties = null) => Log(LogLevel.Trace, message, null, properties);

    /// <summary>
    /// Writes a debug log entry.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="properties">Optional structured properties.</param>
    void Debug(string message, IReadOnlyDictionary<string, object?>? properties = null) => Log(LogLevel.Debug, message, null, properties);

    /// <summary>
    /// Writes an informational log entry.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="properties">Optional structured properties.</param>
    void Info(string message, IReadOnlyDictionary<string, object?>? properties = null) => Log(LogLevel.Information, message, null, properties);

    /// <summary>
    /// Writes a warning log entry.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="properties">Optional structured properties.</param>
    void Warn(string message, IReadOnlyDictionary<string, object?>? properties = null) => Log(LogLevel.Warning, message, null, properties);

    /// <summary>
    /// Writes an error log entry.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="exception">Optional associated exception.</param>
    /// <param name="properties">Optional structured properties.</param>
    void Error(string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null) => Log(LogLevel.Error, message, exception, properties);

    /// <summary>
    /// Writes a critical log entry.
    /// </summary>
    /// <param name="message">Log message.</param>
    /// <param name="exception">Optional associated exception.</param>
    /// <param name="properties">Optional structured properties.</param>
    void Critical(string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null) => Log(LogLevel.Critical, message, exception, properties);
}
