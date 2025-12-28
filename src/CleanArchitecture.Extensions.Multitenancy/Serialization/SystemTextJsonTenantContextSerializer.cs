using System.Text.Json;
using CleanArchitecture.Extensions.Multitenancy.Abstractions;

namespace CleanArchitecture.Extensions.Multitenancy.Serialization;

/// <summary>
/// Serializes tenant context using System.Text.Json.
/// </summary>
public sealed class SystemTextJsonTenantContextSerializer : ITenantContextSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    /// <inheritdoc />
    public string Serialize(TenantContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var snapshot = new TenantContextSnapshot
        {
            Tenant = TenantInfo.From(context.Tenant),
            CorrelationId = context.CorrelationId,
            ResolvedAt = context.ResolvedAt,
            IsValidated = context.IsValidated,
            Source = context.Source,
            Confidence = context.Confidence,
            Candidates = context.Resolution.Candidates.ToArray()
        };

        return JsonSerializer.Serialize(snapshot, SerializerOptions);
    }

    /// <inheritdoc />
    public TenantContext Deserialize(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Payload cannot be empty.", nameof(payload));
        }

        var snapshot = JsonSerializer.Deserialize<TenantContextSnapshot>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("Failed to deserialize tenant context.");

        var resolution = TenantResolutionResult.FromCandidates(snapshot.Candidates, snapshot.Source, snapshot.Confidence);
        var context = new TenantContext(snapshot.Tenant, resolution, snapshot.CorrelationId, snapshot.IsValidated)
        {
            ResolvedAt = snapshot.ResolvedAt
        };

        return context;
    }

    private sealed class TenantContextSnapshot
    {
        public TenantInfo Tenant { get; set; } = new();
        public string? CorrelationId { get; set; }
        public DateTimeOffset ResolvedAt { get; set; }
        public bool IsValidated { get; set; }
        public TenantResolutionSource Source { get; set; }
        public TenantResolutionConfidence Confidence { get; set; }
        public string[] Candidates { get; set; } = Array.Empty<string>();
    }
}
