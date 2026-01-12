using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.Common.Models;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.TodoLists.Queries.GetTodos;

public class TodosVm
{
    public IReadOnlyCollection<LookupDto> PriorityLevels { get; init; } = Array.Empty<LookupDto>();

    public IReadOnlyCollection<TodoListDto> Lists { get; init; } = Array.Empty<TodoListDto>();
}
