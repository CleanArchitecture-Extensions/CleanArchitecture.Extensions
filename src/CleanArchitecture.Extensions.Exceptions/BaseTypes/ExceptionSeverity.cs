namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Indicates the severity of an exception for logging and response mapping.
/// </summary>
public enum ExceptionSeverity
{
    /// <summary>
    /// No severity specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Informational condition that does not represent a failure.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Recoverable warning or client error.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Standard error condition.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Critical failure that likely requires immediate attention.
    /// </summary>
    Critical = 4
}
