using System.Net;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.Providers;

/// <summary>
/// Resolves tenant identifiers from host names.
/// </summary>
public sealed class HostTenantProvider : ITenantProvider
{
    private readonly MultitenancyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HostTenantProvider"/> class.
    /// </summary>
    /// <param name="options">Multitenancy options.</param>
    public HostTenantProvider(IOptions<MultitenancyOptions> options) =>
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public TenantResolutionSource Source => TenantResolutionSource.Host;

    /// <inheritdoc />
    public ValueTask<TenantResolutionResult> ResolveAsync(TenantResolutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.Host))
        {
            return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
        }

        var selector = _options.HostTenantSelector ?? DefaultHostSelector;
        var tenantId = selector(context.Host);

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return ValueTask.FromResult(TenantResolutionResult.NotFound(Source));
        }

        return ValueTask.FromResult(TenantResolutionResult.Resolved(tenantId, Source, TenantResolutionConfidence.Medium));
    }

    private static string? DefaultHostSelector(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var trimmed = host.Trim();
        trimmed = StripPort(trimmed);

        if (IPAddress.TryParse(trimmed, out _))
        {
            return null;
        }

        var segments = trimmed.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length < 2)
        {
            return null;
        }

        return segments[0];
    }

    private static string StripPort(string host)
    {
        if (host.StartsWith("[", StringComparison.Ordinal))
        {
            var endBracket = host.IndexOf(']');
            if (endBracket > 0)
            {
                return host.Substring(1, endBracket - 1);
            }

            return host.Trim('[', ']');
        }

        var colonIndex = host.IndexOf(':');
        return colonIndex > 0 ? host.Substring(0, colonIndex) : host;
    }
}
