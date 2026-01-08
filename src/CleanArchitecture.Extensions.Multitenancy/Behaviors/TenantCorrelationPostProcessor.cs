using MediatR.Pipeline;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// MediatR post-processor that clears tenant correlation scopes when the pre-processor is used.
/// </summary>
public sealed class TenantCorrelationPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantCorrelationScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantCorrelationPostProcessor{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Correlation scope accessor.</param>
    public TenantCorrelationPostProcessor(ITenantCorrelationScopeAccessor scopeAccessor)
    {
        _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
    }

    /// <inheritdoc />
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        _scopeAccessor.ClearScope(onlyIfOwned: true)?.Dispose();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
