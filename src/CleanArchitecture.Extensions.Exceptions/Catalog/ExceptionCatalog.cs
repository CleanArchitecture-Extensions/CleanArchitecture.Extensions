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
            var descriptorFromCatalog = ResolveDescriptorByType(applicationException.GetType()) ?? _unknownDescriptor;
            var mergedMetadata = MergeMetadata(descriptorFromCatalog.Metadata, applicationException.Metadata);
            var message = string.IsNullOrWhiteSpace(descriptorFromCatalog.Message)
                ? _unknownDescriptor.Message
                : descriptorFromCatalog.Message;

            return new ExceptionDescriptor(
                applicationException.GetType(),
                applicationException.Code,
                message,
                applicationException.Severity,
                applicationException.IsTransient,
                applicationException.StatusCode ?? descriptorFromCatalog.StatusCode,
                mergedMetadata);
        }

        return ResolveDescriptorByType(exception.GetType()) ?? _unknownDescriptor;
    }

    private ExceptionDescriptor? ResolveDescriptorByType(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (_descriptors.TryGetValue(current, out var descriptor))
            {
                return descriptor;
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string> MergeMetadata(
        IReadOnlyDictionary<string, string> primary,
        IReadOnlyDictionary<string, string> secondary)
    {
        if (primary.Count == 0 && secondary.Count == 0)
        {
            return primary.Count == 0 ? secondary : primary;
        }

        var merged = new Dictionary<string, string>(primary, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in secondary)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }
}
