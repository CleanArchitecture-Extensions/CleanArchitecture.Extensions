namespace CleanArchitecture.Extensions.Caching.Options;

/// <summary>
/// Defines eviction priority hints for cache entries.
/// </summary>
public enum CachePriority
{
    /// <summary>
    /// Low priority entries are evicted first under pressure.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority entries are evicted after low priority entries.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority entries are evicted after lower priorities.
    /// </summary>
    High = 2,

    /// <summary>
    /// Entries marked as NeverRemove should only be evicted when explicitly removed.
    /// </summary>
    NeverRemove = 3
}
