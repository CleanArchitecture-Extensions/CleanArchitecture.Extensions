using System.Threading;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.Context;

/// <summary>
/// Stores the current tenant context using <see cref="AsyncLocal{T}"/>.
/// </summary>
public sealed class CurrentTenantAccessor : ICurrentTenant, ITenantAccessor
{
    private static readonly AsyncLocal<TenantContextHolder> CurrentHolder = new();

    /// <inheritdoc />
    public TenantContext? Current
    {
        get => CurrentHolder.Value?.Context;
        set
        {
            var holder = CurrentHolder.Value ??= new TenantContextHolder();
            holder.Context = value;
        }
    }

    /// <inheritdoc />
    public string? TenantId => Current?.TenantId;

    /// <inheritdoc />
    public ITenantInfo? TenantInfo => Current?.Tenant;

    /// <inheritdoc />
    public TenantContext? Context => Current;

    /// <inheritdoc />
    public bool IsResolved => !string.IsNullOrWhiteSpace(TenantId);

    /// <inheritdoc />
    public bool IsValidated => Current?.IsValidated ?? false;

    /// <inheritdoc />
    public TenantResolutionSource? Source => Current?.Source;

    /// <inheritdoc />
    public TenantResolutionConfidence Confidence => Current?.Confidence ?? TenantResolutionConfidence.None;

    /// <inheritdoc />
    public IDisposable BeginScope(TenantContext? context)
    {
        var prior = Current;
        Current = context;
        return new TenantContextScope(this, prior);
    }

    private sealed class TenantContextHolder
    {
        public TenantContext? Context { get; set; }
    }

    private sealed class TenantContextScope : IDisposable
    {
        private readonly CurrentTenantAccessor _accessor;
        private readonly TenantContext? _prior;
        private bool _disposed;

        public TenantContextScope(CurrentTenantAccessor accessor, TenantContext? prior)
        {
            _accessor = accessor;
            _prior = prior;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _accessor.Current = _prior;
            _disposed = true;
        }
    }
}
