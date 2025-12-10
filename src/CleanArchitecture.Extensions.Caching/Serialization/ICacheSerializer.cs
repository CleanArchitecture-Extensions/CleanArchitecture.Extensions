namespace CleanArchitecture.Extensions.Caching.Serialization;

/// <summary>
/// Serializer abstraction for cache payloads.
/// </summary>
public interface ICacheSerializer
{
    /// <summary>
    /// Gets the content type produced by the serializer.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Serializes a value to a byte buffer.
    /// </summary>
    byte[] Serialize<T>(T? value);

    /// <summary>
    /// Deserializes a value from a byte buffer.
    /// </summary>
    T? Deserialize<T>(ReadOnlySpan<byte> payload);
}
