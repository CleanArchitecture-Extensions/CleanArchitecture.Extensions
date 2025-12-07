namespace CleanArchitecture.Extensions.Exceptions.Catalog;

/// <summary>
/// Resolves exceptions to catalogued descriptors.
/// </summary>
public interface IExceptionCatalog
{
    /// <summary>
    /// Resolves an exception to a descriptor containing codes and mapping hints.
    /// </summary>
    /// <param name="exception">Exception to resolve.</param>
    /// <returns>Descriptor with code, message, severity, and mapping hints.</returns>
    ExceptionDescriptor Resolve(Exception exception);
}
