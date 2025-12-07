namespace CleanArchitecture.Extensions.Exceptions.BaseTypes;

/// <summary>
/// Well-known exception codes used across the exception catalog and base types.
/// </summary>
public static class ExceptionCodes
{
    /// <summary>
    /// Represents an unknown or uncategorized error.
    /// </summary>
    public const string Unknown = "ERR.UNKNOWN";

    /// <summary>
    /// Represents a generic domain violation.
    /// </summary>
    public const string Domain = "ERR.DOMAIN.GENERIC";

    /// <summary>
    /// Represents a missing resource.
    /// </summary>
    public const string NotFound = "ERR.NOT_FOUND";

    /// <summary>
    /// Represents a resource conflict.
    /// </summary>
    public const string Conflict = "ERR.CONFLICT";

    /// <summary>
    /// Represents a forbidden access or authorization failure.
    /// </summary>
    public const string Forbidden = "ERR.SECURITY.FORBIDDEN";

    /// <summary>
    /// Represents an unauthorized access attempt.
    /// </summary>
    public const string Unauthorized = "ERR.SECURITY.UNAUTHORIZED";

    /// <summary>
    /// Represents validation failures.
    /// </summary>
    public const string Validation = "ERR.VALIDATION";

    /// <summary>
    /// Represents a concurrency conflict.
    /// </summary>
    public const string Concurrency = "ERR.CONCURRENCY";

    /// <summary>
    /// Represents a transient failure that can be retried.
    /// </summary>
    public const string Transient = "ERR.TRANSIENT";

    /// <summary>
    /// Represents a canceled operation.
    /// </summary>
    public const string Cancelled = "ERR.CANCELLED";
}
