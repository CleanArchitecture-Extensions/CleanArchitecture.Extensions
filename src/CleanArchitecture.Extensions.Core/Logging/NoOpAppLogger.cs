namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// Logger implementation that intentionally performs no operations, useful for tests or silent scenarios.
/// </summary>
/// <typeparam name="T">Type used for log category naming.</typeparam>
public sealed class NoOpAppLogger<T> : IAppLogger<T>
{
    /// <summary>
    /// Ignored log call.
    /// </summary>
    /// <param name="level">Severity level.</param>
    /// <param name="message">Log message.</param>
    /// <param name="exception">Optional associated exception.</param>
    /// <param name="properties">Optional structured properties.</param>
    public void Log(LogLevel level, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
    {
        // Intentionally no-op for tests and scenarios where logging is optional.
    }
}
