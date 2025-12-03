using CleanArchitecture.Extensions.Core.Guards.Sample.Domain.Common;

namespace CleanArchitecture.Extensions.Core.Guards.Sample.Domain.Entities;

public class Project : BaseAuditableEntity
{
    public Project(string name, string? description, decimal budget)
    {
        Name = name;
        Description = description?.Trim();
        Budget = budget;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public decimal Budget { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset? ArchivedOn { get; private set; }

    public void Archive(DateTimeOffset archivedOn)
    {
        IsArchived = true;
        ArchivedOn = archivedOn;
    }
}
