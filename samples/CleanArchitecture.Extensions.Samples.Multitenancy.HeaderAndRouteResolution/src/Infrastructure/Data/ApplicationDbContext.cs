using System.Reflection;
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Domain.Entities;
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
