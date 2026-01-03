namespace CleanArchitecture.Extensions.Caching;

/// <summary>
/// Marks a request type as eligible for query caching.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class CacheableQueryAttribute : Attribute
{
}
