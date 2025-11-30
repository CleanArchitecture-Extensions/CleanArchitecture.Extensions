namespace CleanArchitecture.Extensions.Core.Results;

/// <summary>
/// Represents a structured error with code, message, trace identifier, and optional metadata for diagnostics.
/// </summary>
public readonly record struct Error
{
    private static readonly IReadOnlyDictionary<string, string> EmptyMetadata = new Dictionary<string, string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> record.
    /// </summary>
    /// <param name="code">Stable identifier for the error.</param>
    /// <param name="message">Human-readable description of the error.</param>
    /// <param name="traceId">Optional trace identifier used for correlation.</param>
    /// <param name="metadata">Optional set of additional metadata to enrich diagnostics.</param>
    public Error(string code, string message, string? traceId = null, IReadOnlyDictionary<string, string>? metadata = null)
    {
        Code = string.IsNullOrWhiteSpace(code) ? "unknown" : code;
        Message = message ?? string.Empty;
        TraceId = traceId;
        Metadata = metadata ?? EmptyMetadata;
    }

    /// <summary>
    /// Gets the unique error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the descriptive message associated with the error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the trace identifier linked to this error, if any.
    /// </summary>
    public string? TraceId { get; }

    /// <summary>
    /// Gets the metadata attached to this error.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Gets a value indicating whether metadata is present.
    /// </summary>
    public bool HasMetadata => Metadata.Count > 0;

    /// <summary>
    /// Creates a copy of this error with the specified trace identifier.
    /// </summary>
    /// <param name="traceId">The trace identifier to attach.</param>
    /// <returns>A new <see cref="Error"/> instance with the provided trace identifier.</returns>
    public Error WithTraceId(string? traceId) => new(Code, Message, traceId, Metadata);

    /// <summary>
    /// Creates a copy of this error with the provided metadata entry added or replaced.
    /// </summary>
    /// <param name="key">Metadata key.</param>
    /// <param name="value">Metadata value.</param>
    /// <returns>A new <see cref="Error"/> instance containing the merged metadata.</returns>
    public Error WithMetadata(string key, string value)
    {
        var updated = new Dictionary<string, string>(Metadata) { [key] = value };
        return new Error(Code, Message, TraceId, updated);
    }

    /// <summary>
    /// Returns a human-readable representation of the error.
    /// </summary>
    /// <returns>String combining code and message.</returns>
    public override string ToString() => $"{Code}: {Message}";

    /// <summary>
    /// Gets a sentinel instance representing no error.
    /// </summary>
    public static Error None => new("none", string.Empty);
}
