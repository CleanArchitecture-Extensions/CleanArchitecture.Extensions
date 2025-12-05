using CleanArchitecture.Extensions.Core.Time.Sample.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Extensions.Core.Time.Sample.Application.TodoLists.Commands.DeleteTodoList;
using CleanArchitecture.Extensions.Core.Time.Sample.Domain.Entities;

namespace CleanArchitecture.Extensions.Core.Time.Sample.Application.FunctionalTests.TodoLists.Commands;

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
