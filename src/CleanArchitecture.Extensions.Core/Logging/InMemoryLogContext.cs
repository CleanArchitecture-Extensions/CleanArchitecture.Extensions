namespace CleanArchitecture.Extensions.Core.Logging;

/// <summary>
/// In-memory implementation of <see cref="ILogContext"/> that stores properties for inspection in tests.
/// </summary>
public sealed class InMemoryLogContext : ILogContext
{
    private readonly Dictionary<string, object?> _properties = new();
    private readonly object _gate = new();

    /// <summary>
    /// Gets or sets the correlation identifier flowing through log scopes.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets a snapshot of the current properties in the context.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Properties
    {
        get
        {
            lock (_gate)
            {
                return new Dictionary<string, object?>(_properties);
            }
        }
    }

    /// <summary>
    /// Adds a property to the context and returns a disposable scope that removes it when disposed.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    /// <returns>A disposable scope that removes the property when disposed.</returns>
    public IDisposable PushProperty(string name, object? value)
    {
        ArgumentNullException.ThrowIfNull(name);

        lock (_gate)
        {
            _properties[name] = value;
        }

        return new PopScope(this, name);
    }

    private sealed class PopScope : IDisposable
    {
        private readonly InMemoryLogContext _context;
        private readonly string _name;
        private bool _disposed;

        public PopScope(InMemoryLogContext context, string name)
        {
            _context = context;
            _name = name;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            lock (_context._gate)
            {
                _context._properties.Remove(_name);
            }

            _disposed = true;
        }
    }
}
