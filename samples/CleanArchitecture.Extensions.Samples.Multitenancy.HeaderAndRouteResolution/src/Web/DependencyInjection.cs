using Azure.Identity;
// Step 3: (Begin) Multitenancy configuration imports
using CleanArchitecture.Extensions.Multitenancy;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
// Step 3: (End) Multitenancy configuration imports
// Step 4: (Begin) Multitenancy ASP.NET Core registration imports
using CleanArchitecture.Extensions.Multitenancy.AspNetCore;
// Step 4: (End) Multitenancy ASP.NET Core registration imports
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Infrastructure.Data;
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Web.Services;
using Microsoft.AspNetCore.Mvc;

using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        // Step 7: (Begin) Register ProblemDetails for exception handling
        builder.Services.AddProblemDetails();
        // Step 7: (End) Register ProblemDetails for exception handling

        // Step 3: (Begin) Configure multitenancy resolution defaults
        builder.Services.Configure<MultitenancyOptions>(options =>
        {
            options.RequireTenantByDefault = true;
            options.AllowAnonymous = true;
            options.HeaderNames = new[] { "X-Tenant-ID" };
            options.ResolutionOrder = new List<TenantResolutionSource>
            {
                TenantResolutionSource.Route,
                TenantResolutionSource.Host,
                TenantResolutionSource.Header,
                TenantResolutionSource.QueryString,
                TenantResolutionSource.Claim
            };
            options.FallbackTenant = null;
            options.FallbackTenantId = null;
        });
        // Step 3: (End) Configure multitenancy resolution defaults

        // Step 4: (Begin) Register multitenancy services and ASP.NET Core adapter
        builder.Services.AddCleanArchitectureMultitenancy();
        builder.Services.AddCleanArchitectureMultitenancyAspNetCore(autoUseMiddleware: false);
        // Step 4: (End) Register multitenancy services and ASP.NET Core adapter


        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution API";

            // Add JWT
            configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
