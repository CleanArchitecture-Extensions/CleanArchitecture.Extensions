namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// Log context implementation that does nothing, preserving call sites that expect a context.
/// </summary>
public sealed class NoOpLogContext : ILogContext
{
    private sealed class DisposableScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Gets or sets the correlation identifier; unused for no-op scenarios.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// No-op scope creation for log properties.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    /// <returns>A disposable that performs no work.</returns>
    public IDisposable PushProperty(string name, object? value) => new DisposableScope();
}
