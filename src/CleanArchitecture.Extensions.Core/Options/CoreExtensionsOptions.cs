using CleanArchitecture.Extensions.Core.Guards;

namespace CleanArchitecture.Extensions.Core.Options;

/// <summary>
/// Represents configuration options for the Core extensions package.
/// </summary>
public sealed class CoreExtensionsOptions
{
    /// <summary>
    /// Gets or sets the header name used for correlation identifiers in transports.
    /// </summary>
    public string CorrelationHeaderName { get; set; } = "X-Correlation-ID";

    /// <summary>
    /// Gets or sets the default guard strategy used across the package.
    /// </summary>
    public GuardStrategy GuardStrategy { get; set; } = GuardStrategy.ReturnFailure;

    /// <summary>
    /// Gets or sets a value indicating whether performance logging is enabled.
    /// </summary>
    public bool EnablePerformanceLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the duration threshold that triggers performance warnings.
    /// </summary>
    public TimeSpan PerformanceWarningThreshold { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the factory used to generate new correlation identifiers when missing.
    /// </summary>
    public Func<string> CorrelationIdFactory { get; set; } = () => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets a resolver used to pull an existing correlation identifier from the current context (e.g., HTTP headers).
    /// </summary>
    public Func<string?>? CorrelationIdResolver { get; set; }

    /// <summary>
    /// Gets or sets the trace identifier applied to results and guard errors.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static CoreExtensionsOptions Default => new();
}
