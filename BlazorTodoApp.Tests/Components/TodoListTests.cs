using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using BlazorTodoApp.Components;
using BlazorTodoApp.Models;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Xunit;

namespace BlazorTodoApp.Tests.Components;

public class TodoListTests
{
    [Fact]
    public void RendersVirtualizeWhenExceedingThreshold()
    {
        using var ctx = new TestContext();
        var items = Enumerable.Range(0, 100)
            .Select(i => TodoItem.Rehydrate(
                Guid.NewGuid(),
                $"Item {i}",
                null,
                false,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null))
            .ToList();

        var cut = ctx.RenderComponent<TodoList>(parameters => parameters
            .Add(p => p.Items, items));

        Assert.NotEmpty(cut.FindComponents<Virtualize<TodoItem>>());
    }

    [Fact]
    public void RendersForEachWhenBelowThreshold()
    {
        using var ctx = new TestContext();
        var items = new List<TodoItem>
        {
            TodoItem.Rehydrate(Guid.NewGuid(), "A", null, false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null)
        };

        var cut = ctx.RenderComponent<TodoList>(parameters => parameters
            .Add(p => p.Items, items));

        Assert.Empty(cut.FindComponents<Virtualize<TodoItem>>());
        Assert.Single(cut.FindAll("[data-test='todo-row']"));
    }
}
