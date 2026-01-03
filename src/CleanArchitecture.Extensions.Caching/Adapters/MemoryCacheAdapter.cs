using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Internal;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Caching.Adapters;

/// <summary>
/// Memory-backed cache adapter with basic stampede protection.
/// </summary>
public sealed class MemoryCacheAdapter : ICache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheSerializer _serializer;
    private readonly ILogger<MemoryCacheAdapter> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly CachingOptions _options;
    private readonly KeyedLock _locks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheAdapter"/> class.
    /// </summary>
    /// <param name="memoryCache">Memory cache instance.</param>
    /// <param name="serializer">Serializer used to persist values.</param>
    /// <param name="timeProvider">Time provider used for timestamps.</param>
    /// <param name="options">Caching options.</param>
    /// <param name="logger">Logger.</param>
    public MemoryCacheAdapter(
        IMemoryCache memoryCache,
        ICacheSerializer serializer,
        TimeProvider timeProvider,
        IOptions<CachingOptions> options,
        ILogger<MemoryCacheAdapter> logger)
        : this(memoryCache, new[] { serializer }, timeProvider, options, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheAdapter"/> class.
    /// </summary>
    /// <param name="memoryCache">Memory cache instance.</param>
    /// <param name="serializers">Serializers registered for cache payloads.</param>
    /// <param name="timeProvider">Time provider used for timestamps.</param>
    /// <param name="options">Caching options.</param>
    /// <param name="logger">Logger.</param>
    [ActivatorUtilitiesConstructor]
    public MemoryCacheAdapter(
        IMemoryCache memoryCache,
        IEnumerable<ICacheSerializer> serializers,
        TimeProvider timeProvider,
        IOptions<CachingOptions> options,
        ILogger<MemoryCacheAdapter> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializer = CacheSerializerSelector.Select(serializers, _options);
    }

    /// <inheritdoc />
    public CacheItem<T?>? Get<T>(CacheKey key)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        if (_memoryCache.TryGetValue(key.FullKey, out StoredCacheEntry? stored) && stored is not null)
        {
            var value = DeserializeValue<T>(stored);
            return new CacheItem<T?>(key, value, stored.CreatedAt, stored.ExpiresAt, stored.ContentType, stored.Options);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<CacheItem<T?>?> GetAsync<T>(CacheKey key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Get<T>(key);
    }

    /// <inheritdoc />
    public void Set<T>(CacheKey key, T value, CacheEntryOptions? options = null)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var entryOptions = ResolveEntryOptions(options);
        var payload = _serializer.Serialize(value);

        if (ExceedsSizeLimit(payload, entryOptions))
        {
            _logger.LogWarning("Cache entry for {Key} exceeded maximum size; skipping store.", key.FullKey);
            return;
        }

        var memoryOptions = ToMemoryOptions(entryOptions);
        var createdAt = _timeProvider.GetUtcNow();
        var expiresAt = ResolveExpiresAt(createdAt, memoryOptions);
        var stored = new StoredCacheEntry(payload, _serializer.ContentType, createdAt, expiresAt, entryOptions, value);

        _memoryCache.Set(key.FullKey, stored, memoryOptions);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(CacheKey key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Set(key, value, options);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public T? GetOrAdd<T>(CacheKey key, Func<T> factory, CacheEntryOptions? options = null, CacheStampedePolicy? stampedePolicy = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var cached = Get<T>(key);
        if (cached is not null)
        {
            return cached.Value;
        }

        if (!_options.Enabled)
        {
            return factory();
        }

        var policy = stampedePolicy ?? _options.StampedePolicy ?? CacheStampedePolicy.Default;

        if (policy.EnableLocking)
        {
            var gate = _locks.TryAcquire(key.FullKey, policy.LockTimeout);
            if (gate is null)
            {
                _logger.LogWarning("Failed to acquire cache lock for {Key} within {Timeout}ms; bypassing cache.", key.FullKey, policy.LockTimeout.TotalMilliseconds);
                return factory();
            }

            try
            {
                cached = Get<T>(key);
                if (cached is not null)
                {
                    return cached.Value;
                }

                var value = factory();
                Set(key, value, options);
                return value;
            }
            finally
            {
                gate.Value.Dispose();
            }
        }
        else
        {
            var value = factory();
            Set(key, value, options);
            return value;
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetOrAddAsync<T>(CacheKey key, Func<CancellationToken, Task<T>> factory, CacheEntryOptions? options = null, CacheStampedePolicy? stampedePolicy = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        cancellationToken.ThrowIfCancellationRequested();

        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return cached.Value;
        }

        if (!_options.Enabled)
        {
            return await factory(cancellationToken).ConfigureAwait(false);
        }

        var policy = stampedePolicy ?? _options.StampedePolicy ?? CacheStampedePolicy.Default;

        if (policy.EnableLocking)
        {
            var gate = await _locks.TryAcquireAsync(key.FullKey, policy.LockTimeout, cancellationToken).ConfigureAwait(false);
            if (gate is null)
            {
                _logger.LogWarning("Failed to acquire cache lock for {Key} within {Timeout}ms; bypassing cache.", key.FullKey, policy.LockTimeout.TotalMilliseconds);
                return await factory(cancellationToken).ConfigureAwait(false);
            }

            try
            {
                cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
                if (cached is not null)
                {
                    return cached.Value;
                }

                var value = await factory(cancellationToken).ConfigureAwait(false);
                await SetAsync(key, value, options, cancellationToken).ConfigureAwait(false);
                return value;
            }
            finally
            {
                gate.Value.Dispose();
            }
        }
        else
        {
            var value = await factory(cancellationToken).ConfigureAwait(false);
            await SetAsync(key, value, options, cancellationToken).ConfigureAwait(false);
            return value;
        }
    }

    /// <inheritdoc />
    public void Remove(CacheKey key)
    {
        _memoryCache.Remove(key.FullKey);
    }

    /// <inheritdoc />
    public Task RemoveAsync(CacheKey key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Remove(key);
        return Task.CompletedTask;
    }

    private MemoryCacheEntryOptions ToMemoryOptions(CacheEntryOptions entryOptions)
    {
        var memoryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = entryOptions.AbsoluteExpiration,
            AbsoluteExpirationRelativeToNow = ApplyJitter(entryOptions.AbsoluteExpirationRelativeToNow, _options.StampedePolicy?.Jitter),
            SlidingExpiration = entryOptions.SlidingExpiration,
            Size = entryOptions.Size,
            Priority = MapPriority(entryOptions.Priority)
        };

        return memoryOptions;
    }

    private static CacheItemPriority MapPriority(CachePriority priority) => priority switch
    {
        CachePriority.Low => CacheItemPriority.Low,
        CachePriority.High => CacheItemPriority.High,
        CachePriority.NeverRemove => CacheItemPriority.NeverRemove,
        _ => CacheItemPriority.Normal
    };

    private CacheEntryOptions ResolveEntryOptions(CacheEntryOptions? options) => options ?? _options.DefaultEntryOptions ?? CacheEntryOptions.Default;

    private bool ExceedsSizeLimit(byte[] payload, CacheEntryOptions entryOptions)
    {
        if (_options.MaxEntrySizeBytes.HasValue && payload.LongLength > _options.MaxEntrySizeBytes.Value)
        {
            return true;
        }

        if (entryOptions.Size.HasValue && _options.MaxEntrySizeBytes.HasValue && entryOptions.Size.Value > _options.MaxEntrySizeBytes.Value)
        {
            return true;
        }

        return false;
    }

    private DateTimeOffset? ResolveExpiresAt(DateTimeOffset createdAt, MemoryCacheEntryOptions options)
    {
        if (options.AbsoluteExpiration.HasValue)
        {
            return options.AbsoluteExpiration;
        }

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            return createdAt.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        return null;
    }

    private T? DeserializeValue<T>(StoredCacheEntry stored)
    {
        if (stored.Value is T typed)
        {
            return typed;
        }

        try
        {
            return _serializer.Deserialize<T>(stored.Payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cached value for content type {ContentType}", stored.ContentType ?? "unknown");
            return default;
        }
    }

    private static TimeSpan? ApplyJitter(TimeSpan? baseValue, TimeSpan? jitter)
    {
        if (!baseValue.HasValue)
        {
            return null;
        }

        if (!jitter.HasValue || jitter.Value <= TimeSpan.Zero)
        {
            return baseValue;
        }

        var jitterMilliseconds = jitter.Value.TotalMilliseconds;
        var offsetMs = Random.Shared.NextDouble() * jitterMilliseconds;
        return baseValue.Value + TimeSpan.FromMilliseconds(offsetMs);
    }

    private sealed record StoredCacheEntry(byte[] Payload, string? ContentType, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt, CacheEntryOptions Options, object? Value);
}
