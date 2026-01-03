# Reference: Caching options

## CachingOptions

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `Enabled` | `bool` | `true` | Enables or disables caching globally. |
| `DefaultNamespace` | `string` | `"CleanArchitectureExtensions"` | Namespace applied to cache keys. |
| `DefaultEntryOptions` | `CacheEntryOptions` | `CacheEntryOptions.Default` | Default entry options when no overrides are provided. |
| `StampedePolicy` | `CacheStampedePolicy` | `CacheStampedePolicy.Default` | Locking and jitter defaults. |
| `MaxEntrySizeBytes` | `long?` | `null` | Maximum payload size in bytes. |
| `PreferredSerializer` | `string?` | `null` | Preferred serializer type name or content type when multiple serializers are registered. |

## QueryCachingBehaviorOptions

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `CachePredicate` | `Func<object,bool>` | `ICacheableQuery` or `[CacheableQuery]` | Determines cacheable requests. |
| `ResourceNameSelector` | `Func<object,string>?` | `null` | Custom resource name for cache keys. |
| `HashFactory` | `Func<object,string>?` | `null` | Custom hash for cache keys. |
| `DefaultTtl` | `TimeSpan?` | `00:05:00` | Default TTL for cached queries. |
| `TtlByRequestType` | `Dictionary<Type,TimeSpan>` | empty | Per-request TTL overrides. |
| `CacheNullValues` | `bool` | `true` | Cache null responses. |
| `ResponseCachePredicate` | `Func<object,object?,bool>?` | `null` | Decide whether a response should be cached. |

## CacheEntryOptions

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `AbsoluteExpirationRelativeToNow` | `TimeSpan?` | `null` | Absolute expiration relative to now. |
| `AbsoluteExpiration` | `DateTimeOffset?` | `null` | Absolute expiration timestamp. |
| `SlidingExpiration` | `TimeSpan?` | `null` | Sliding expiration interval. |
| `Priority` | `CachePriority` | `Normal` | Eviction priority (when supported). |
| `Size` | `long?` | `null` | Size hint for providers that enforce size limits. |

## CacheStampedePolicy

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `EnableLocking` | `bool` | `true` | Enable per-key locking. |
| `LockTimeout` | `TimeSpan` | `00:00:05` | Max wait for a lock. |
| `Jitter` | `TimeSpan?` | `00:00:00.050` | Random jitter applied to expirations. |
