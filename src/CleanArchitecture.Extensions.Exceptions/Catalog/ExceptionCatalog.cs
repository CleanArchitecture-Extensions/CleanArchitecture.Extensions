using CleanArchitecture.Extensions.Exceptions.BaseTypes;
using CleanArchitecture.Extensions.Exceptions.Options;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Exceptions.Catalog;

/// <summary>
/// Default implementation of <see cref="IExceptionCatalog"/> that merges defaults with user-provided descriptors.
/// </summary>
public sealed class ExceptionCatalog : IExceptionCatalog
{
    private readonly IReadOnlyDictionary<Type, ExceptionDescriptor> _descriptors;
    private readonly ExceptionDescriptor _unknownDescriptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionCatalog"/> class.
    /// </summary>
    /// <param name="options">Optional catalog customization.</param>
    public ExceptionCatalog(IOptions<ExceptionCatalogOptions>? options = null)
    {
        var merged = new Dictionary<Type, ExceptionDescriptor>(ExceptionCatalogDefaults.Descriptors);

        if (options?.Value?.Descriptors is { Count: > 0 })
        {
            foreach (var descriptor in options.Value.Descriptors)
            {
                if (descriptor is null || descriptor.ExceptionType is null)
                {
                    continue;
                }

                merged[descriptor.ExceptionType] = descriptor;
            }
        }

        _descriptors = merged;
        _unknownDescriptor = ExceptionCatalogDefaults.Unknown;
    }

    /// <inheritdoc />
    public ExceptionDescriptor Resolve(Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (exception is ApplicationExceptionBase applicationException)
        {
            return ExceptionDescriptor.FromApplicationException(applicationException);
        }

        var current = exception.GetType();
        while (current is not null)
        {
            if (_descriptors.TryGetValue(current, out var descriptor))
            {
                return descriptor;
            }

            current = current.BaseType;
        }

        return _unknownDescriptor;
    }
}
