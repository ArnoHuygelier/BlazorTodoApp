using Bunit;
using BlazorTodoApp.Components;
using BlazorTodoApp.Models;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace BlazorTodoApp.Tests.Components;

public class TodoFiltersTests
{
    [Fact]
    public void ClickingButtonInvokesCallback()
    {
        using var ctx = new TestContext();
        TodoFilterOption? captured = null;

        var cut = ctx.RenderComponent<TodoFilters>(parameters => parameters
            .Add(p => p.SelectedFilter, TodoFilterOption.All)
            .Add(p => p.TotalCount, 5)
            .Add(p => p.ActiveCount, 2)
            .Add(p => p.CompletedCount, 3)
            .Add(p => p.OnFilterSelected, option => captured = option));

        cut.Find("[data-test='filter-completed']").Click();

        Assert.Equal(TodoFilterOption.Completed, captured);
    }

    [Fact]
    public void SelectedButtonReflectsPressedState()
    {
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<TodoFilters>(parameters => parameters
            .Add(p => p.SelectedFilter, TodoFilterOption.Active)
            .Add(p => p.TotalCount, 5)
            .Add(p => p.ActiveCount, 4)
            .Add(p => p.CompletedCount, 1));

        var activeButton = cut.Find("[data-test='filter-active']");
        var allButton = cut.Find("[data-test='filter-all']");

        Assert.Equal("true", activeButton.GetAttribute("aria-pressed"));
        Assert.Equal("false", allButton.GetAttribute("aria-pressed"));
    }

    [Fact]
    public void KeyboardShortcutChangesFilter()
    {
        using var ctx = new TestContext();
        TodoFilterOption? captured = null;

        var cut = ctx.RenderComponent<TodoFilters>(parameters => parameters
            .Add(p => p.SelectedFilter, TodoFilterOption.All)
            .Add(p => p.TotalCount, 5)
            .Add(p => p.ActiveCount, 2)
            .Add(p => p.CompletedCount, 3)
            .Add(p => p.OnFilterSelected, option => captured = option));

        cut.Find("[data-test='todo-filters']").KeyDown(new KeyboardEventArgs { Key = "2" });

        Assert.Equal(TodoFilterOption.Active, captured);
    }
}
