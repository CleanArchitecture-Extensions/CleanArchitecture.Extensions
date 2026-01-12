using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.TodoLists.Commands.DeleteTodoList;
using CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Domain.Entities;

namespace CleanArchitecture.Extensions.Samples.Multitenancy.HeaderAndRouteResolution.Application.FunctionalTests.TodoLists.Commands;

using static Testing;

public class DeleteTodoListTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoListId()
    {
        var command = new DeleteTodoListCommand(99);
        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldDeleteTodoList()
    {
        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        await SendAsync(new DeleteTodoListCommand(listId));

        var list = await FindAsync<TodoList>(listId);

        list.ShouldBeNull();
    }
}
