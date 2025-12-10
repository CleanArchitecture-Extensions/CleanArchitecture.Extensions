using System.Text.Json;

namespace CleanArchitecture.Extensions.Caching.Serialization;

/// <summary>
/// Default cache serializer using System.Text.Json.
/// </summary>
public sealed class SystemTextJsonCacheSerializer : ICacheSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonCacheSerializer"/> class.
    /// </summary>
    /// <param name="options">Optional serializer options; defaults to <see cref="JsonSerializerDefaults.Web"/>.</param>
    public SystemTextJsonCacheSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public string ContentType => "application/json";

    /// <inheritdoc />
    public byte[] Serialize<T>(T? value) => JsonSerializer.SerializeToUtf8Bytes(value, _options);

    /// <inheritdoc />
    public T? Deserialize<T>(ReadOnlySpan<byte> payload) => JsonSerializer.Deserialize<T>(payload, _options);
}
