using CleanArchitecture.Extensions.Core.Result.Sample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1024);

        builder.Property(p => p.Budget)
            .HasPrecision(18, 2);

        builder.Property(p => p.IsClosed)
            .HasDefaultValue(false);
    }
}
