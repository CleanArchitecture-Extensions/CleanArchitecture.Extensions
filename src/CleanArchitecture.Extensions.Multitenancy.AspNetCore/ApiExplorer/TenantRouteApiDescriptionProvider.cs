using System.Reflection;
using System.Text.RegularExpressions;
using CleanArchitecture.Extensions.Multitenancy.AspNetCore.Options;
using CleanArchitecture.Extensions.Multitenancy.Configuration;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Extensions.Multitenancy.AspNetCore.ApiExplorer;

/// <summary>
/// Ensures tenant route parameters appear in API descriptions when they are not bound by handlers.
/// </summary>
public sealed class TenantRouteApiDescriptionProvider : IApiDescriptionProvider
{
    private static readonly Regex DisplayNameRouteRegex = new(
        @"(?:^|HTTP:\s*)(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS)\s+(?<path>/\S+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly MultitenancyOptions _options;
    private readonly AspNetCoreMultitenancyOptions _aspNetCoreOptions;
    private readonly IModelMetadataProvider? _modelMetadataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantRouteApiDescriptionProvider"/> class.
    /// </summary>
    /// <param name="options">Multitenancy options.</param>
    /// <param name="aspNetCoreOptions">ASP.NET Core multitenancy options.</param>
    /// <param name="modelMetadataProvider">Model metadata provider.</param>
    public TenantRouteApiDescriptionProvider(
        IOptions<MultitenancyOptions> options,
        IOptions<AspNetCoreMultitenancyOptions> aspNetCoreOptions,
        IModelMetadataProvider? modelMetadataProvider = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _aspNetCoreOptions = aspNetCoreOptions?.Value ?? throw new ArgumentNullException(nameof(aspNetCoreOptions));
        _modelMetadataProvider = modelMetadataProvider;
    }

    /// <inheritdoc />
    public int Order => 1000;

    /// <inheritdoc />
    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!_aspNetCoreOptions.EnableOpenApiIntegration)
        {
            return;
        }

        foreach (var description in context.Results)
        {
            var template = GetTemplate(description.ActionDescriptor);
            if (!ShouldApplyTemplate(template))
            {
                continue;
            }

            var normalizedTemplate = NormalizeTemplate(template!);
            if (!ContainsRouteParameter(normalizedTemplate, _options.RouteParameterName))
            {
                continue;
            }

            var relativePath = description.RelativePath;
            if (!string.IsNullOrWhiteSpace(relativePath)
                && !ContainsRouteParameter(relativePath, _options.RouteParameterName))
            {
                description.RelativePath = normalizedTemplate;
            }

            EnsurePathParameter(description, _options.RouteParameterName);
        }
    }

    /// <inheritdoc />
    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
    }

    private static bool ShouldApplyTemplate(string? template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return false;
        }

        return template.IndexOf('[', StringComparison.OrdinalIgnoreCase) < 0;
    }

    private static string NormalizeTemplate(string template)
    {
        return template.TrimStart('/');
    }

    private static string? GetTemplate(ActionDescriptor? actionDescriptor)
    {
        if (actionDescriptor is null)
        {
            return null;
        }

        var template = actionDescriptor.AttributeRouteInfo?.Template;
        if (!string.IsNullOrWhiteSpace(template))
        {
            return template;
        }

        return TryGetRoutePatternTemplate(actionDescriptor)
            ?? TryGetTemplateFromEndpointMetadata(actionDescriptor)
            ?? TryGetTemplateFromDisplayName(actionDescriptor);
    }

    private static string? TryGetRoutePatternTemplate(ActionDescriptor actionDescriptor)
    {
        var routePatternProperty = actionDescriptor.GetType().GetProperty(
            "RoutePattern",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (routePatternProperty is null)
        {
            return null;
        }

        var routePattern = routePatternProperty.GetValue(actionDescriptor);
        return GetRawText(routePattern);
    }

    private static string? TryGetTemplateFromEndpointMetadata(ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor.EndpointMetadata is null || actionDescriptor.EndpointMetadata.Count == 0)
        {
            return null;
        }

        foreach (var metadata in actionDescriptor.EndpointMetadata)
        {
            if (metadata is null)
            {
                continue;
            }

            var routePatternProperty = metadata.GetType().GetProperty(
                "RoutePattern",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (routePatternProperty is not null)
            {
                var routePattern = routePatternProperty.GetValue(metadata);
                var template = GetRawText(routePattern);
                if (!string.IsNullOrWhiteSpace(template))
                {
                    return template;
                }
            }

            var templateProperty = metadata.GetType().GetProperty(
                "Template",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (templateProperty?.PropertyType == typeof(string))
            {
                var template = templateProperty.GetValue(metadata) as string;
                if (!string.IsNullOrWhiteSpace(template))
                {
                    return template;
                }
            }
        }

        return null;
    }

    private static string? GetRawText(object? routePattern)
    {
        if (routePattern is null)
        {
            return null;
        }

        var rawTextProperty = routePattern.GetType().GetProperty(
            "RawText",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (rawTextProperty?.PropertyType == typeof(string))
        {
            return rawTextProperty.GetValue(routePattern) as string;
        }

        return routePattern.ToString();
    }

    private static string? TryGetTemplateFromDisplayName(ActionDescriptor actionDescriptor)
    {
        var displayName = actionDescriptor.DisplayName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        var match = DisplayNameRouteRegex.Match(displayName);
        if (!match.Success)
        {
            return null;
        }

        return match.Groups["path"].Value;
    }

    private static bool ContainsRouteParameter(string template, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(parameterName))
        {
            return false;
        }

        var token = "{" + parameterName;
        var index = template.IndexOf(token, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return false;
        }

        var nextIndex = index + token.Length;
        if (nextIndex >= template.Length)
        {
            return false;
        }

        var nextChar = template[nextIndex];
        return nextChar == '}' || nextChar == ':' || nextChar == '?';
    }

    private void EnsurePathParameter(ApiDescription description, string parameterName)
    {
        if (description.ParameterDescriptions.Any(parameter =>
                parameter.Source == BindingSource.Path
                && string.Equals(parameter.Name, parameterName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var parameter = new ApiParameterDescription
        {
            Name = parameterName,
            Source = BindingSource.Path,
            Type = typeof(string),
            IsRequired = true
        };

        if (_modelMetadataProvider is not null)
        {
            parameter.ModelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(string));
        }

        description.ParameterDescriptions.Add(parameter);
    }
}
