using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Catalog;

namespace CleanArchitecture.Extensions.Exceptions.Classification;

/// <summary>
/// Provides helpers to classify exceptions for retry and resilience policies.
/// </summary>
public static class ExceptionClassifier
{
    /// <summary>
    /// Determines whether an exception is transient/retryable based on the catalog and known types.
    /// </summary>
    /// <param name="exception">Exception to inspect.</param>
    /// <param name="catalog">Catalog used for descriptors.</param>
    /// <returns>True when the exception is transient.</returns>
    public static bool IsTransient(Exception exception, IExceptionCatalog catalog)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (catalog is null)
        {
            throw new ArgumentNullException(nameof(catalog));
        }

        if (exception is ApplicationExceptionBase appException)
        {
            return appException.IsTransient;
        }

        if (exception is TimeoutException)
        {
            return true;
        }

        var descriptor = catalog.Resolve(exception);
        return descriptor.IsTransient;
    }
}
