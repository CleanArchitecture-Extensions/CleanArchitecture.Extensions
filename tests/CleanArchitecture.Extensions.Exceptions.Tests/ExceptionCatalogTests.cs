using System.Net;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Options;

namespace CleanArchitecture.Extensions.Exceptions.Tests;

/// <summary>
/// Tests for the exception catalog resolution logic.
/// </summary>
public class ExceptionCatalogTests
{
    private readonly ExceptionCatalog _catalog = new();

    [Fact]
    public void Resolve_ReturnsDescriptor_ForApplicationException()
    {
        var descriptor = _catalog.Resolve(new NotFoundException("Order", 42));

        Assert.Equal(ExceptionCodes.NotFound, descriptor.Code);
        Assert.Equal(ExceptionSeverity.Error, descriptor.Severity);
        Assert.Equal(HttpStatusCode.NotFound, descriptor.StatusCode);
    }

    [Fact]
    public void Resolve_ReturnsUnauthorizedDescriptor_ForUnauthorizedAccessException()
    {
        var descriptor = _catalog.Resolve(new UnauthorizedAccessException());

        Assert.Equal(ExceptionCodes.Unauthorized, descriptor.Code);
        Assert.Equal(ExceptionSeverity.Error, descriptor.Severity);
        Assert.Equal(HttpStatusCode.Unauthorized, descriptor.StatusCode);
    }

    [Fact]
    public void Resolve_UsesCatalogMessageForApplicationException()
    {
        const string message = "Resource Order with id 7 was not found.";
        var descriptor = _catalog.Resolve(new NotFoundException(message));

        Assert.Equal("The specified resource was not found.", descriptor.Message);
    }

    [Fact]
    public void Resolve_UsesCustomDescriptorWhenConfigured()
    {
        var options = new ExceptionCatalogOptions();
        options.Descriptors.Add(new ExceptionDescriptor(typeof(InvalidOperationException), "custom.code", "custom message", ExceptionSeverity.Warning));
        var catalog = new ExceptionCatalog(Microsoft.Extensions.Options.Options.Create(options));

        var descriptor = catalog.Resolve(new InvalidOperationException("oops"));

        Assert.Equal("custom.code", descriptor.Code);
        Assert.Equal("custom message", descriptor.Message);
        Assert.Equal(ExceptionSeverity.Warning, descriptor.Severity);
    }

    [Fact]
    public void Resolve_MergesApplicationExceptionMetadata()
    {
        var appException = new DomainException("domain.code", "message", metadata: new Dictionary<string, string> { ["meta"] = "value" });

        var descriptor = _catalog.Resolve(appException);

        Assert.Equal("value", descriptor.Metadata["meta"]);
        Assert.Equal("domain.code", descriptor.Code);
    }

    [Fact]
    public void Resolve_MarksConcurrencyAsTransient()
    {
        var descriptor = _catalog.Resolve(new ConcurrencyException());

        Assert.True(descriptor.IsTransient);
        Assert.Equal(ExceptionSeverity.Warning, descriptor.Severity);
    }

    [Fact]
    public void Resolve_UnknownException_UsesFallbackDescriptor()
    {
        var descriptor = _catalog.Resolve(new InvalidOperationException("boom"));

        Assert.Equal(ExceptionCodes.Unknown, descriptor.Code);
        Assert.Equal(ExceptionSeverity.Error, descriptor.Severity);
        Assert.Null(descriptor.StatusCode);
    }
}
