using System;
using System.Collections.Generic;
using CleanArchitecture.Extensions.Multitenancy.EFCore.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace CleanArchitecture.Extensions.Multitenancy.EFCore;

internal sealed class TenantModelCacheKeyOptionsExtension : IDbContextOptionsExtension
{
    private readonly EfCoreMultitenancyOptions _options;
    private DbContextOptionsExtensionInfo? _info;

    public TenantModelCacheKeyOptionsExtension(EfCoreMultitenancyOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton<IOptions<EfCoreMultitenancyOptions>>(OptionsFactory.Create(_options));
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private readonly TenantModelCacheKeyOptionsExtension _extension;

        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
            _extension = (TenantModelCacheKeyOptionsExtension)extension;
        }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "using TenantModelCacheKeyFactory ";

        public override int GetServiceProviderHashCode()
            => HashCode.Combine(
                _extension._options.Mode,
                _extension._options.IncludeSchemaInModelCacheKey,
                _extension._options.SchemaNameFormat,
                _extension._options.DefaultSchema,
                _extension._options.SchemaNameProvider?.GetHashCode() ?? 0);

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            if (other is not ExtensionInfo otherInfo)
            {
                return false;
            }

            var otherOptions = otherInfo._extension._options;

            return _extension._options.Mode == otherOptions.Mode
                && _extension._options.IncludeSchemaInModelCacheKey == otherOptions.IncludeSchemaInModelCacheKey
                && string.Equals(_extension._options.SchemaNameFormat, otherOptions.SchemaNameFormat, StringComparison.Ordinal)
                && string.Equals(_extension._options.DefaultSchema, otherOptions.DefaultSchema, StringComparison.Ordinal)
                && Equals(_extension._options.SchemaNameProvider, otherOptions.SchemaNameProvider);
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["CleanArchitecture.Extensions.Multitenancy.EFCore:TenantModelCacheKey"] = "1";
        }
    }
}
