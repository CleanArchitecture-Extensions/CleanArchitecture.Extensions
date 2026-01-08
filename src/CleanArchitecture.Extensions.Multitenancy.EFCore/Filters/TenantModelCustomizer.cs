using System.Linq.Expressions;
using System.Reflection;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Abstractions;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore.Filters;

/// <summary>
/// Applies tenant query filters and schema configuration.
/// </summary>
public sealed class TenantModelCustomizer : ITenantModelCustomizer
{
    private static readonly MethodInfo PropertyMethod = typeof(EF).GetMethods()
        .Single(method => method.Name == nameof(EF.Property)
            && method.IsGenericMethodDefinition
            && method.GetParameters().Length == 2);

    /// <inheritdoc />
    public void Customize(ModelBuilder modelBuilder, ITenantDbContext context, EfCoreMultitenancyOptions options)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        if (options.Mode == TenantIsolationMode.SchemaPerTenant)
        {
            var schema = options.ResolveSchemaName(context.CurrentTenantInfo);
            if (!string.IsNullOrWhiteSpace(schema))
            {
                modelBuilder.HasDefaultSchema(schema);
            }
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!TenantEntityMetadata.IsTenantScoped(entityType, options))
            {
                continue;
            }

            var builder = modelBuilder.Entity(entityType.ClrType!);
            var tenantProperty = entityType.FindProperty(options.TenantIdPropertyName);
            if (tenantProperty is null)
            {
                if (options.UseShadowTenantId)
                {
                    builder.Property<string>(options.TenantIdPropertyName);
                    tenantProperty = entityType.FindProperty(options.TenantIdPropertyName);
                }
                else if (options.Mode == TenantIsolationMode.SharedDatabase)
                {
                    throw new InvalidOperationException(
                        $"Tenant property '{options.TenantIdPropertyName}' was not found on '{entityType.ClrType!.Name}'. " +
                        "Add the property, enable UseShadowTenantId, or mark the entity as global.");
                }
                else
                {
                    continue;
                }
            }

            if (!options.EnableQueryFilters || tenantProperty is null)
            {
                continue;
            }

            var filter = BuildTenantFilter(entityType.ClrType!, context, options);
            foreach (var queryFilter in entityType.GetDeclaredQueryFilters())
            {
                if (queryFilter.Expression is LambdaExpression existingFilter)
                {
                    filter = CombineFilters(existingFilter, filter);
                }
            }

            builder.HasQueryFilter(filter);
        }
    }

    private static LambdaExpression BuildTenantFilter(Type entityType, ITenantDbContext context, EfCoreMultitenancyOptions options)
    {
        var parameter = Expression.Parameter(entityType, "entity");
        var propertyAccess = Expression.Call(
            PropertyMethod.MakeGenericMethod(typeof(string)),
            parameter,
            Expression.Constant(options.TenantIdPropertyName));

        var tenantId = BuildTenantIdExpression(context);
        if (tenantId.Type != typeof(string))
        {
            tenantId = Expression.Convert(tenantId, typeof(string));
        }

        var equals = Expression.Equal(propertyAccess, tenantId);
        return Expression.Lambda(equals, parameter);
    }

    private static Expression BuildTenantIdExpression(ITenantDbContext context)
    {
        var contextType = context.GetType();
        var tenantProperty = contextType.GetProperty(nameof(ITenantDbContext.CurrentTenantId));
        if (tenantProperty is not null)
        {
            return Expression.Property(Expression.Constant(context), tenantProperty);
        }

        var interfaceProperty = typeof(ITenantDbContext).GetProperty(nameof(ITenantDbContext.CurrentTenantId));
        return Expression.Property(Expression.Constant(context, typeof(ITenantDbContext)), interfaceProperty!);
    }

    private static LambdaExpression CombineFilters(LambdaExpression existing, LambdaExpression added)
    {
        var parameter = existing.Parameters[0];
        var replaced = new ParameterReplaceVisitor(added.Parameters[0], parameter).Visit(added.Body);
        var body = Expression.AndAlso(existing.Body, replaced ?? added.Body);
        return Expression.Lambda(body, parameter);
    }

    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _source)
            {
                return _target;
            }

            return base.VisitParameter(node);
        }
    }
}
