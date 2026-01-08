using System.Threading;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// Provides access to the current tenant correlation logging scope.
/// </summary>
public interface ITenantCorrelationScopeAccessor
{
    /// <summary>
    /// Gets the current logging scope, if any.
    /// </summary>
    IDisposable? CurrentScope { get; }

    /// <summary>
    /// Gets a value indicating whether the current scope is owned by this component.
    /// </summary>
    bool IsOwned { get; }

    /// <summary>
    /// Sets the current logging scope and ownership flag.
    /// </summary>
    /// <param name="scope">Scope instance.</param>
    /// <param name="owned">Whether the scope should be disposed by this accessor.</param>
    void SetScope(IDisposable? scope, bool owned);

    /// <summary>
    /// Clears the current scope, optionally only when it is owned.
    /// </summary>
    /// <param name="onlyIfOwned">True to clear only when the scope is owned.</param>
    /// <returns>The cleared scope, if any.</returns>
    IDisposable? ClearScope(bool onlyIfOwned = false);
}

internal sealed class TenantCorrelationScopeAccessor : ITenantCorrelationScopeAccessor
{
    private static readonly AsyncLocal<TenantCorrelationScopeState?> CurrentState = new();

    public IDisposable? CurrentScope => CurrentState.Value?.Scope;

    public bool IsOwned => CurrentState.Value?.Owned ?? false;

    public void SetScope(IDisposable? scope, bool owned)
    {
        CurrentState.Value = new TenantCorrelationScopeState(scope, owned);
    }

    public IDisposable? ClearScope(bool onlyIfOwned = false)
    {
        var state = CurrentState.Value;
        if (state is null)
        {
            return null;
        }

        if (onlyIfOwned && !state.Owned)
        {
            return null;
        }

        CurrentState.Value = null;
        return state.Scope;
    }

    private sealed record TenantCorrelationScopeState(IDisposable? Scope, bool Owned);
}
