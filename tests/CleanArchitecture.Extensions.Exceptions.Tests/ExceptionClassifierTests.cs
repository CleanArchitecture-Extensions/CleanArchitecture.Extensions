using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;
using CleanArchitecture.Extensions.Exceptions.Classification;

namespace CleanArchitecture.Extensions.Exceptions.Tests;

/// <summary>
/// Tests covering transient classification helpers.
/// </summary>
public class ExceptionClassifierTests
{
    [Fact]
    public void IsTransient_True_ForApplicationTransient()
    {
        var catalog = new ExceptionCatalog();
        var result = ExceptionClassifier.IsTransient(new TransientException(), catalog);

        Assert.True(result);
    }

    [Fact]
    public void IsTransient_True_ForTimeoutException()
    {
        var catalog = new ExceptionCatalog();

        var result = ExceptionClassifier.IsTransient(new TimeoutException(), catalog);

        Assert.True(result);
    }

    [Fact]
    public void IsTransient_UsesCatalogForOtherExceptions()
    {
        var catalog = new ExceptionCatalog();

        var result = ExceptionClassifier.IsTransient(new ConcurrencyException(), catalog);

        Assert.True(result);
    }

    [Fact]
    public void IsTransient_ThrowsWhenInputsNull()
    {
        var catalog = new ExceptionCatalog();

        Assert.Throws<ArgumentNullException>(() => ExceptionClassifier.IsTransient(null!, catalog));
        Assert.Throws<ArgumentNullException>(() => ExceptionClassifier.IsTransient(new Exception(), null!));
    }
}
