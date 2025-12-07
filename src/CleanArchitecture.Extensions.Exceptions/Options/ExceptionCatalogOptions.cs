using CleanArchitecture.Extensions.Exceptions.Catalog;

namespace CleanArchitecture.Extensions.Exceptions.Options;

/// <summary>
/// Configures the exception catalog with custom descriptors.
/// </summary>
public sealed class ExceptionCatalogOptions
{
    /// <summary>
    /// Gets custom descriptors that override or extend the defaults.
    /// </summary>
    public IList<ExceptionDescriptor> Descriptors { get; } = new List<ExceptionDescriptor>();

    /// <summary>
    /// Gets the default options instance.
    /// </summary>
    public static ExceptionCatalogOptions Default => new();
}
