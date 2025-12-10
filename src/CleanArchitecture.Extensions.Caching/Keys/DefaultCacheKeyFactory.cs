using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CleanArchitecture.Extensions.Caching.Options;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Caching.Keys;

/// <summary>
/// Default cache key factory using JSON hashing for deterministic keys.
/// </summary>
public sealed class DefaultCacheKeyFactory : ICacheKeyFactory
{
    private static readonly JsonSerializerOptions HashSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly CachingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultCacheKeyFactory"/> class.
    /// </summary>
    /// <param name="options">Caching options.</param>
    public DefaultCacheKeyFactory(IOptions<CachingOptions> options) =>
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc />
    public CacheKey Create(string resource, string hash, string? tenantId = null, string? cacheNamespace = null)
    {
        var ns = NormalizeNamespace(cacheNamespace);
        var normalizedResource = string.IsNullOrWhiteSpace(resource) ? throw new ArgumentException("Resource cannot be empty.", nameof(resource)) : resource.Trim();
        var normalizedHash = string.IsNullOrWhiteSpace(hash) ? throw new ArgumentException("Hash cannot be empty.", nameof(hash)) : hash.Trim();
        var normalizedTenant = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();

        return new CacheKey(ns, normalizedResource, normalizedHash, normalizedTenant);
    }

    /// <inheritdoc />
    public CacheKey FromRequest<TRequest>(TRequest request, string? tenantId = null, string? cacheNamespace = null)
    {
        var resource = typeof(TRequest).Name;
        var hash = CreateHash(request);
        return Create(resource, hash, tenantId, cacheNamespace);
    }

    /// <inheritdoc />
    public string CreateHash(object? parameters)
    {
        // Null parameters map to a stable token.
        if (parameters is null)
        {
            return "null";
        }

        var json = JsonSerializer.Serialize(parameters, HashSerializerOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <inheritdoc />
    public string NormalizeNamespace(string? cacheNamespace)
    {
        if (string.IsNullOrWhiteSpace(cacheNamespace))
        {
            if (string.IsNullOrWhiteSpace(_options.DefaultNamespace))
            {
                throw new InvalidOperationException("CachingOptions.DefaultNamespace must be configured.");
            }

            return _options.DefaultNamespace.Trim();
        }

        return cacheNamespace.Trim();
    }
}
