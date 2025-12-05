using CleanArchitecture.Extensions.Core.DomainEvents;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Constants;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.Data;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.Data.Interceptors;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.DomainEvents;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("CleanArchitecture.Extensions.Core.DomainEvents.SampleDb");
        Guard.Against.Null(connectionString, message: "Connection string 'CleanArchitecture.Extensions.Core.DomainEvents.SampleDb' not found.");

        builder.Services.AddSingleton<IDomainEventLog, InMemoryDomainEventLog>();
        builder.Services.AddScoped<DomainEventTracker>();
        builder.Services.AddScoped<MediatorDomainEventDispatcher>();
        builder.Services.AddScoped<IDomainEventDispatcher>(sp =>
        {
            var mediatorDispatcher = sp.GetRequiredService<MediatorDomainEventDispatcher>();
            var log = sp.GetRequiredService<IDomainEventLog>();
            return new RecordingDomainEventDispatcher(mediatorDispatcher, log);
        });

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }
}
