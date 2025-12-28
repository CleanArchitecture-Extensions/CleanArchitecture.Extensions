namespace CleanArchitecture.Extensions.Multitenancy.Tests;

public class TenantResolutionResultTests
{
    [Fact]
    public void Resolved_trims_and_sets_confidence()
    {
        var result = TenantResolutionResult.Resolved(" tenant ", TenantResolutionSource.Header, TenantResolutionConfidence.High);

        Assert.True(result.IsResolved);
        Assert.False(result.IsAmbiguous);
        Assert.Equal("tenant", result.TenantId);
        Assert.Equal(TenantResolutionConfidence.High, result.Confidence);
        Assert.Single(result.Candidates);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolved_throws_when_tenant_id_empty(string? tenantId)
    {
        Assert.Throws<ArgumentException>(() =>
            TenantResolutionResult.Resolved(tenantId!, TenantResolutionSource.Header));
    }

    [Fact]
    public void FromCandidates_normalizes_and_marks_ambiguous()
    {
        var result = TenantResolutionResult.FromCandidates(
            new[] { "TenantA", " tenanta ", "TenantB", " " },
            TenantResolutionSource.Header);

        Assert.False(result.IsResolved);
        Assert.True(result.IsAmbiguous);
        Assert.Null(result.TenantId);
        Assert.Equal(2, result.Candidates.Count);
        Assert.Equal(TenantResolutionConfidence.Low, result.Confidence);
    }

    [Fact]
    public void FromCandidates_returns_not_found_when_empty()
    {
        var result = TenantResolutionResult.FromCandidates(Array.Empty<string>(), TenantResolutionSource.Header);

        Assert.False(result.IsResolved);
        Assert.False(result.IsAmbiguous);
        Assert.Empty(result.Candidates);
        Assert.Equal(TenantResolutionConfidence.None, result.Confidence);
    }
}
