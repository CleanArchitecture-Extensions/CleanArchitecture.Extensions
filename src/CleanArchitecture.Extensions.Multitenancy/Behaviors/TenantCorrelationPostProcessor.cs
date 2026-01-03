using MediatR.Pipeline;

namespace CleanArchitecture.Extensions.Multitenancy.Behaviors;

/// <summary>
/// MediatR post-processor that clears tenant correlation scopes when the pre-processor is used.
/// </summary>
public sealed class TenantCorrelationPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        TenantCorrelationScope.Clear()?.Dispose();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
