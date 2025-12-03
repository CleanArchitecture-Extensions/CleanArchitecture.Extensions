using CleanArchitecture.Extensions.Core.Result.Sample.Domain.Common;

namespace CleanArchitecture.Extensions.Core.Result.Sample.Domain.Entities;

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

    public bool IsClosed { get; private set; }

    public DateTimeOffset? ClosedOn { get; private set; }

    public void Close(DateTimeOffset closedOn)
    {
        IsClosed = true;
        ClosedOn = closedOn;
    }
}
