using System.Reflection;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Application.Common.Interfaces;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Domain.Entities;
using CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Extensions.Core.DomainEvents.Sample.Infrastructure.Data;

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
