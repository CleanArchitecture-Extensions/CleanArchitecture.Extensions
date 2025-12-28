namespace CleanArchitecture.Extensions.Multitenancy;

/// <summary>
/// Represents the outcome of a tenant resolution attempt.
/// </summary>
public sealed class TenantResolutionResult
{
    private TenantResolutionResult(
        TenantResolutionSource source,
        TenantResolutionConfidence confidence,
        IReadOnlyList<string> candidates)
    {
        Source = source;
        Confidence = confidence;
        Candidates = candidates;
        TenantId = candidates.Count == 1 ? candidates[0] : null;
    }

    /// <summary>
    /// Gets the resolution source.
    /// </summary>
    public TenantResolutionSource Source { get; }

    /// <summary>
    /// Gets the resolution confidence.
    /// </summary>
    public TenantResolutionConfidence Confidence { get; }

    /// <summary>
    /// Gets the resolved tenant identifier, if a single candidate exists.
    /// </summary>
    public string? TenantId { get; }

    /// <summary>
    /// Gets the candidate tenant identifiers.
    /// </summary>
    public IReadOnlyList<string> Candidates { get; }

    /// <summary>
    /// Gets a value indicating whether a single tenant was resolved.
    /// </summary>
    public bool IsResolved => TenantId is not null;

    /// <summary>
    /// Gets a value indicating whether multiple candidates were returned.
    /// </summary>
    public bool IsAmbiguous => Candidates.Count > 1;

    /// <summary>
    /// Creates a result that indicates no tenant was found.
    /// </summary>
    /// <param name="source">Resolution source.</param>
    public static TenantResolutionResult NotFound(TenantResolutionSource source) =>
        new(source, TenantResolutionConfidence.None, Array.Empty<string>());

    /// <summary>
    /// Creates a resolved result for a single tenant identifier.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="source">Resolution source.</param>
    /// <param name="confidence">Resolution confidence.</param>
    public static TenantResolutionResult Resolved(
        string tenantId,
        TenantResolutionSource source,
        TenantResolutionConfidence confidence = TenantResolutionConfidence.Medium)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant identifier cannot be empty.", nameof(tenantId));
        }

        return new TenantResolutionResult(source, confidence, new[] { tenantId.Trim() });
    }

    /// <summary>
    /// Creates a result from candidate identifiers.
    /// </summary>
    /// <param name="candidates">Candidate identifiers.</param>
    /// <param name="source">Resolution source.</param>
    /// <param name="confidence">Resolution confidence.</param>
    public static TenantResolutionResult FromCandidates(
        IEnumerable<string>? candidates,
        TenantResolutionSource source,
        TenantResolutionConfidence confidence = TenantResolutionConfidence.Medium)
    {
        var normalized = NormalizeCandidates(candidates);

        if (normalized.Count == 0)
        {
            return NotFound(source);
        }

        var resultConfidence = normalized.Count == 1 ? confidence : TenantResolutionConfidence.Low;
        return new TenantResolutionResult(source, resultConfidence, normalized);
    }

    private static IReadOnlyList<string> NormalizeCandidates(IEnumerable<string>? candidates)
    {
        if (candidates is null)
        {
            return Array.Empty<string>();
        }

        var normalized = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var trimmed = candidate.Trim();
            if (seen.Add(trimmed))
            {
                normalized.Add(trimmed);
            }
        }

        return normalized;
    }
}
