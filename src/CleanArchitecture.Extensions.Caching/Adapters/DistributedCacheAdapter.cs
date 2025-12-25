using System.Collections.Concurrent;
using CleanArchitecture.Extensions.Caching.Abstractions;
using CleanArchitecture.Extensions.Caching.Keys;
using CleanArchitecture.Extensions.Caching.Options;
using CleanArchitecture.Extensions.Caching.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Caching.Adapters;

/// <summary>
/// Distributed cache adapter (IDistributedCache) with stampede protection.
/// </summary>
public sealed class DistributedCacheAdapter : ICache
{
    private readonly IDistributedCache _distributedCache;
    private readonly ICacheSerializer _serializer;
    private readonly ILogger<DistributedCacheAdapter> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly CachingOptions _options;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheAdapter"/> class.
    /// </summary>
    /// <param name="distributedCache">Distributed cache instance.</param>
    /// <param name="serializer">Serializer for payloads.</param>
    /// <param name="options">Caching options.</param>
    /// <param name="timeProvider">Time provider used for timestamps.</param>
    /// <param name="logger">Logger.</param>
    public DistributedCacheAdapter(
        IDistributedCache distributedCache,
        ICacheSerializer serializer,
        IOptions<CachingOptions> options,
        TimeProvider timeProvider,
        ILogger<DistributedCacheAdapter> logger)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public CacheItem<T?>? Get<T>(CacheKey key)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var bytes = _distributedCache.Get(key.FullKey);
        if (bytes is null)
        {
            return null;
        }

        var stored = DeserializeStored<T>(bytes);
        if (stored is null)
        {
            return null;
        }

        return new CacheItem<T?>(key, stored.Value, stored.CreatedAt, stored.ExpiresAt, stored.ContentType, stored.Options);
    }

    /// <inheritdoc />
    public async Task<CacheItem<T?>?> GetAsync<T>(CacheKey key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_options.Enabled)
        {
            return null;
        }

        var bytes = await _distributedCache.GetAsync(key.FullKey, cancellationToken).ConfigureAwait(false);
        if (bytes is null)
        {
            return null;
        }

        var stored = DeserializeStored<T>(bytes);
        if (stored is null)
        {
            return null;
        }

        return new CacheItem<T?>(key, stored.Value, stored.CreatedAt, stored.ExpiresAt, stored.ContentType, stored.Options);
    }

    /// <inheritdoc />
    public void Set<T>(CacheKey key, T value, CacheEntryOptions? options = null)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var entryOptions = ResolveEntryOptions(options);
        var envelope = CreateEnvelope(value, entryOptions);
        var payload = _serializer.Serialize(envelope);

        if (ExceedsSizeLimit(payload))
        {
            _logger.LogWarning("Cache entry for {Key} exceeded maximum size; skipping store.", key.FullKey);
            return;
        }

        var distributedOptions = ToDistributedOptions(entryOptions);
        _distributedCache.Set(key.FullKey, payload, distributedOptions);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(CacheKey key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_options.Enabled)
        {
            return Task.CompletedTask;
        }

        var entryOptions = ResolveEntryOptions(options);
        var envelope = CreateEnvelope(value, entryOptions);
        var payload = _serializer.Serialize(envelope);

        if (ExceedsSizeLimit(payload))
        {
            _logger.LogWarning("Cache entry for {Key} exceeded maximum size; skipping store.", key.FullKey);
            return Task.CompletedTask;
        }

        var distributedOptions = ToDistributedOptions(entryOptions);
        return _distributedCache.SetAsync(key.FullKey, payload, distributedOptions, cancellationToken);
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
            var gate = GetLock(key.FullKey);
            if (!gate.Wait(policy.LockTimeout))
            {
                _logger.LogWarning("Failed to acquire distributed cache lock for {Key} within {Timeout}ms; bypassing cache.", key.FullKey, policy.LockTimeout.TotalMilliseconds);
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
                gate.Release();
            }
        }

        var result = factory();
        Set(key, result, options);
        return result;
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
            var gate = GetLock(key.FullKey);
            if (!await gate.WaitAsync(policy.LockTimeout, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogWarning("Failed to acquire distributed cache lock for {Key} within {Timeout}ms; bypassing cache.", key.FullKey, policy.LockTimeout.TotalMilliseconds);
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
                gate.Release();
            }
        }

        var result = await factory(cancellationToken).ConfigureAwait(false);
        await SetAsync(key, result, options, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public void Remove(CacheKey key) => _distributedCache.Remove(key.FullKey);

    /// <inheritdoc />
    public Task RemoveAsync(CacheKey key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _distributedCache.RemoveAsync(key.FullKey, cancellationToken);
    }

    private CacheEntryOptions ResolveEntryOptions(CacheEntryOptions? options) => options ?? _options.DefaultEntryOptions ?? CacheEntryOptions.Default;

    private bool ExceedsSizeLimit(byte[] payload) => _options.MaxEntrySizeBytes.HasValue && payload.LongLength > _options.MaxEntrySizeBytes.Value;

    private DistributedCacheEntryOptions ToDistributedOptions(CacheEntryOptions options) => new()
    {
        AbsoluteExpiration = options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow = ApplyJitter(options.AbsoluteExpirationRelativeToNow, _options.StampedePolicy?.Jitter),
        SlidingExpiration = options.SlidingExpiration
    };

    private DistributedStoredEntry<T> CreateEnvelope<T>(T value, CacheEntryOptions options)
    {
        var createdAt = _timeProvider.GetUtcNow();
        DateTimeOffset? expiresAt = null;
        if (options.AbsoluteExpiration.HasValue)
        {
            expiresAt = options.AbsoluteExpiration;
        }
        else if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            expiresAt = createdAt.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        return new DistributedStoredEntry<T>(value, createdAt, expiresAt, options);
    }

    private DistributedStoredEntry<T>? DeserializeStored<T>(byte[] bytes)
    {
        try
        {
            return _serializer.Deserialize<DistributedStoredEntry<T>>(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize distributed cache entry");
            return null;
        }
    }

    private SemaphoreSlim GetLock(string key) => _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

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

    private sealed record DistributedStoredEntry<T>(T? Value, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt, CacheEntryOptions Options)
    {
        public string? ContentType => "application/json";
    }
}
