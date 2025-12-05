using CleanArchitecture.Extensions.Core.Options.Sample.Application.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Extensions.Core.Options.Sample.Application.TodoItems.Commands.DeleteTodoItem;
using CleanArchitecture.Extensions.Core.Options.Sample.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Extensions.Core.Options.Sample.Domain.Entities;

namespace CleanArchitecture.Extensions.Core.Options.Sample.Application.FunctionalTests.TodoItems.Commands;

using static Testing;

public class DeleteTodoItemTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoItemId()
    {
        var command = new DeleteTodoItemCommand(99);

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldDeleteTodoItem()
    {
        var listId = await SendAsync(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var itemId = await SendAsync(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "New Item"
        });

        await SendAsync(new DeleteTodoItemCommand(itemId));

        var item = await FindAsync<TodoItem>(itemId);

        item.ShouldBeNull();
    }
}
