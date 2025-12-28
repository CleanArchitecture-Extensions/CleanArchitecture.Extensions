namespace CleanArchitecture.Extensions.Multitenancy.Abstractions;

/// <summary>
/// Manages the ambient tenant context.
/// </summary>
public interface ITenantAccessor
{
    /// <summary>
    /// Gets or sets the current tenant context.
    /// </summary>
    TenantContext? Current { get; set; }

    /// <summary>
    /// Begins a scoped tenant context that restores the previous context on dispose.
    /// </summary>
    /// <param name="context">Tenant context to apply for the scope.</param>
    /// <returns>Disposable scope.</returns>
    IDisposable BeginScope(TenantContext? context);
}
