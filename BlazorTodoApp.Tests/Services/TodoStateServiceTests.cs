using BlazorTodoApp.Models;
using BlazorTodoApp.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BlazorTodoApp.Tests.Services;

public class TodoStateServiceTests
{
    private readonly Mock<ITodoRepository> repository = new();
    private readonly TodoStateService service;

    public TodoStateServiceTests()
    {
        repository.Setup(r => r.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TodoItem>());
        repository.Setup(r => r.LoadFilterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(TodoFilter.Default);
        repository.Setup(r => r.SaveAsync(It.IsAny<IReadOnlyList<TodoItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        service = new TodoStateService(repository.Object, NullLogger<TodoStateService>.Instance);
    }

    [Fact]
    public async Task AddTodoAsync_PersistsNewItem()
    {
        var added = await service.AddTodoAsync("Write docs", "Ensure coverage", null);

        Assert.NotEqual(Guid.Empty, added.Id);
        Assert.False(added.IsCompleted);
        Assert.Single(service.Items);
        Assert.Equal("Write docs", service.Items[0].Title);
        repository.Verify(r => r.SaveAsync(It.IsAny<IReadOnlyList<TodoItem>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoAsync_ModifiesExistingItem()
    {
        var seed = TodoItem.Create("Plan sprint", "Initial", null, DateTimeOffset.UtcNow);
        repository.Setup(r => r.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { seed });

        await service.InitializeAsync();
        await service.UpdateTodoAsync(seed.Id, "Plan release", "Add retrospective", null);

        var updated = Assert.Single(service.Items);
        Assert.Equal("Plan release", updated.Title);
        Assert.Equal("Add retrospective", updated.Note);
        repository.Verify(r => r.SaveAsync(It.Is<IReadOnlyList<TodoItem>>(items => items.Single().Title == "Plan release"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleTodoAsync_FlipsCompletionState()
    {
        var seed = TodoItem.Create("File expenses", null, null, DateTimeOffset.UtcNow);
        repository.Setup(r => r.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { seed });

        await service.InitializeAsync();
        await service.ToggleTodoAsync(seed.Id);

        Assert.True(service.Items[0].IsCompleted);
        repository.Verify(r => r.SaveAsync(It.IsAny<IReadOnlyList<TodoItem>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoAsync_RemovesItem()
    {
        var keep = TodoItem.Create("Keep", null, null, DateTimeOffset.UtcNow);
        var remove = TodoItem.Create("Remove", null, null, DateTimeOffset.UtcNow);
        repository.Setup(r => r.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { keep, remove });

        await service.InitializeAsync();
        await service.DeleteTodoAsync(remove.Id);

        var remaining = Assert.Single(service.Items);
        Assert.Equal(keep.Id, remaining.Id);
        repository.Verify(r => r.SaveAsync(It.Is<IReadOnlyList<TodoItem>>(items => items.Single().Id == keep.Id), It.IsAny<CancellationToken>()), Times.Once);
    }
}
