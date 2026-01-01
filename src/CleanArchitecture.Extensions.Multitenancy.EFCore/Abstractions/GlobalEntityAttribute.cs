namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;

/// <summary>
/// Marks an entity as global and exempt from tenant filtering.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GlobalEntityAttribute : Attribute
{
}
