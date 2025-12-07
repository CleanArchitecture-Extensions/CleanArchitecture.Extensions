namespace CleanArchitecture.Extensions.Exceptions.Options;

/// <summary>
/// Configures how exceptions are wrapped, logged, and translated to results.
/// </summary>
public sealed class ExceptionHandlingOptions
{
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
