using System.Net;
using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;

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
    public void Resolve_UsesApplicationExceptionMessage()
    {
        const string message = "Resource Order with id 7 was not found.";
        var descriptor = _catalog.Resolve(new NotFoundException(message));

        Assert.Equal(message, descriptor.Message);
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
