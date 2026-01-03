using CleanArchitecture.Extensions.Caching.Options;

namespace CleanArchitecture.Extensions.Caching.Serialization;

internal static class CacheSerializerSelector
{
    public static ICacheSerializer Select(IEnumerable<ICacheSerializer> serializers, CachingOptions options)
    {
        ArgumentNullException.ThrowIfNull(serializers);
        ArgumentNullException.ThrowIfNull(options);

        var serializerList = serializers.ToList();
        if (serializerList.Count == 0)
        {
            throw new InvalidOperationException("No cache serializers are registered.");
        }

        if (!string.IsNullOrWhiteSpace(options.PreferredSerializer))
        {
            var preferred = options.PreferredSerializer.Trim();
            var match = serializerList.FirstOrDefault(serializer =>
                string.Equals(serializer.ContentType, preferred, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(serializer.GetType().Name, preferred, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(serializer.GetType().FullName, preferred, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                throw new InvalidOperationException($"Preferred cache serializer '{preferred}' was not found.");
            }

            return match;
        }

        return serializerList[^1];
    }
}
