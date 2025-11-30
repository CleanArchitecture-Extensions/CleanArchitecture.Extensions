namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// Defines a lightweight log context used to enrich structured logs.
/// </summary>
public interface ILogContext
{
    /// <summary>
    /// Gets or sets the correlation identifier used for end-to-end tracing.
    /// </summary>
    string? CorrelationId { get; set; }

    /// <summary>
    /// Pushes a property into the logging scope.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    /// <returns>An <see cref="IDisposable"/> that removes the property when disposed.</returns>
    IDisposable PushProperty(string name, object? value);
}
