using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Attributes;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.Routing;

/// <summary>
/// Endpoint metadata helpers for multitenancy.
/// </summary>
public static class EndpointConventionBuilderExtensions
{
    /// <summary>
    /// Marks the endpoint as requiring a tenant.
    /// </summary>
    /// <param name="builder">Endpoint convention builder.</param>
    public static TBuilder RequireTenant<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithMetadata(new RequiresTenantAttribute());
        return builder;
    }

    /// <summary>
    /// Marks the endpoint as allowing tenant-less requests.
    /// </summary>
    /// <param name="builder">Endpoint convention builder.</param>
    public static TBuilder AllowAnonymousTenant<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithMetadata(new AllowAnonymousTenantAttribute());
        return builder;
    }

    /// <summary>
    /// Adds the multitenancy enforcement filter to the endpoint.
    /// </summary>
    /// <param name="builder">Route handler builder.</param>
    public static RouteHandlerBuilder AddTenantEnforcement(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddEndpointFilter<TenantEnforcementEndpointFilter>();
        return builder;
    }

    /// <summary>
    /// Adds the multitenancy enforcement filter to the endpoint group.
    /// </summary>
    /// <param name="builder">Route group builder.</param>
    public static RouteGroupBuilder AddTenantEnforcement(this RouteGroupBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddEndpointFilter<TenantEnforcementEndpointFilter>();
        return builder;
    }

    /// <summary>
    /// Adds tenant header metadata to the endpoint.
    /// </summary>
    /// <param name="builder">Endpoint convention builder.</param>
    /// <param name="headerName">Header name.</param>
    public static TBuilder WithTenantHeader<TBuilder>(this TBuilder builder, string headerName)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithMetadata(new TenantHeaderAttribute(headerName));
        return builder;
    }

    /// <summary>
    /// Adds tenant route metadata to the endpoint.
    /// </summary>
    /// <param name="builder">Endpoint convention builder.</param>
    /// <param name="routeParameterName">Route parameter name.</param>
    public static TBuilder WithTenantRoute<TBuilder>(this TBuilder builder, string routeParameterName)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithMetadata(new TenantRouteAttribute(routeParameterName));
        return builder;
    }
}
