namespace CleanArchitecture.Extensions.Exceptions.Options;

using System.Net;

/// <summary>
/// Configures how exceptions are wrapped, logged, and translated to results.
/// </summary>
public sealed class ExceptionHandlingOptions
{
    /// <summary>
    /// Gets or sets the current environment name (e.g., Development, Production) used for detail/stack toggles.
    /// </summary>
    public string? EnvironmentName { get; set; }

    /// <summary>
    /// Gets the environments in which exception details are included automatically.
    /// </summary>
    public ISet<string> IncludeExceptionDetailsEnvironments { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Development" };

    /// <summary>
    /// Gets or sets a value indicating whether exceptions should be converted to <see cref="CleanArchitecture.Extensions.Core.Results.Result"/> when the response type supports it.
    /// </summary>
    public bool ConvertToResult { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether exception messages should flow to responses/logs instead of default catalog messages.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }
        = false;

    /// <summary>
    /// Gets or sets a value indicating whether stack traces should be logged/passed through when details are included.
    /// </summary>
    public bool IncludeStackTrace { get; set; }
        = false;

    /// <summary>
    /// Gets the environments in which stack traces are included automatically.
    /// </summary>
    public ISet<string> IncludeStackTraceEnvironments { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Development" };

    /// <summary>
    /// Gets or sets a value indicating whether sensitive data should be redacted from exception details and metadata.
    /// </summary>
    public bool RedactSensitiveData { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether cancellation exceptions should be rethrown instead of wrapped.
    /// </summary>
    public bool RethrowCancellationExceptions { get; set; } = true;

    /// <summary>
    /// Gets the set of exception types that must bypass wrapping.
    /// </summary>
    public ISet<Type> RethrowExceptionTypes { get; } = CreateDefaultRethrowTypes();

    /// <summary>
    /// Gets or sets an explicit trace identifier to attach to results.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether exceptions should be logged.
    /// </summary>
    public bool LogExceptions { get; set; } = true;

    /// <summary>
    /// Gets the overrides for mapping error codes to HTTP status codes.
    /// </summary>
    public IDictionary<string, HttpStatusCode> StatusCodeOverrides { get; } = new Dictionary<string, HttpStatusCode>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static ExceptionHandlingOptions Default => new();

    private static HashSet<Type> CreateDefaultRethrowTypes()
    {
        var types = new HashSet<Type> { typeof(OperationCanceledException) };

        AddIfPresent(types, "CleanArchitecture.Extensions.Validation.Exceptions.ValidationException, CleanArchitecture.Extensions.Validation");
        AddIfPresent(types, "FluentValidation.ValidationException, FluentValidation");

        return types;
    }

    private static void AddIfPresent(ISet<Type> set, string typeName)
    {
        var type = Type.GetType(typeName, throwOnError: false);
        if (type is not null)
        {
            set.Add(type);
        }
    }
}
