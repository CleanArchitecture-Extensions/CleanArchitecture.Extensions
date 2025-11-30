namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// Defines severity levels for application logging.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Detailed diagnostic information used primarily for development.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Debug-level information that is less verbose than trace.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Information messages that track the general flow of the application.
    /// </summary>
    Information = 2,

    /// <summary>
    /// Potentially harmful situations that warrant attention.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error events that might still allow the application to continue.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Critical errors causing premature termination or major failure.
    /// </summary>
    Critical = 5,

    /// <summary>
    /// Special level indicating logging is disabled.
    /// </summary>
    None = 6
}
