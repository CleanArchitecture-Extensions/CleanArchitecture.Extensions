using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;

internal static class TenantEntityMetadata
{
    public static bool IsTenantScoped(IReadOnlyEntityType entityType, EfCoreMultitenancyOptions options)
    {
        if (entityType.ClrType is null)
        {
            return false;
        }

        if (entityType.IsOwned() || entityType.FindPrimaryKey() is null)
        {
            return false;
        }

        return !IsGlobalEntity(entityType.ClrType, options);
    }

    public static bool IsGlobalEntity(Type clrType, EfCoreMultitenancyOptions options)
    {
        if (options.TreatIdentityEntitiesAsGlobal && IsIdentityEntity(clrType))
        {
            return true;
        }

        if (typeof(IGlobalEntity).IsAssignableFrom(clrType))
        {
            return true;
        }

        if (Attribute.IsDefined(clrType, typeof(GlobalEntityAttribute)))
        {
            return true;
        }

        if (options.GlobalEntityTypes.Contains(clrType))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(clrType.FullName) && options.GlobalEntityTypeNames.Contains(clrType.FullName))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(clrType.Name) && options.GlobalEntityTypeNames.Contains(clrType.Name);
    }

    private static bool IsIdentityEntity(Type clrType)
    {
        const string identityNamespace = "Microsoft.AspNetCore.Identity";

        for (var current = clrType; current is not null; current = current.BaseType)
        {
            var ns = current.Namespace;
            if (!string.IsNullOrWhiteSpace(ns) && ns.StartsWith(identityNamespace, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
