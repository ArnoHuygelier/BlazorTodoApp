using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using BlazorTodoApp.Models;
using BlazorTodoApp.Pages;
using BlazorTodoApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorTodoApp.Tests.Components;

public class TodoDashboardTests
{
    [Fact]
    public void AddTodoFlow_DisplaysNewRow()
    {
        using var ctx = CreateContext();
        var cut = ctx.RenderComponent<TodoDashboard>();

        cut.Find("[data-test='add-todo-button']").Click();
        cut.Find("#todo-title").Change("Plan sprint");
        cut.Find("#todo-note").Change("Discuss blockers");
        cut.Find("[data-test='save-todo-button']").Click();

        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("[data-test='todo-row']");
            Assert.Single(rows);
            Assert.Contains("Plan sprint", rows[0].TextContent);
        });
    }

    [Fact]
    public void EditFlow_UpdatesExistingRow()
    {
        var seed = new[]
        {
            TodoItem.Rehydrate(
                Guid.Parse("7d0e1517-efb6-4c1a-9d2c-7d4bd2e7d0a1"),
                "Initial title",
                "Initial note",
                false,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null)
        };

        using var ctx = CreateContext(seed);
        var cut = ctx.RenderComponent<TodoDashboard>();

        cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-test='todo-row']")));
        cut.Find("[data-test='edit-button']").Click();
        cut.Find("#todo-title").Change("Updated title");
        cut.Find("#todo-note").Change("Updated note");
        cut.Find("[data-test='save-todo-button']").Click();

        cut.WaitForAssertion(() =>
        {
            var row = cut.Find("[data-test='todo-row']");
            Assert.Contains("Updated title", row.TextContent);
            Assert.Contains("Updated note", row.TextContent);
        });
    }

    [Fact]
    public void ToggleFlow_AppliesCompletedStyling()
    {
        var seed = new[]
        {
            TodoItem.Rehydrate(
                Guid.Parse("f67ebde6-8c0c-42d2-9e4c-2d064a390133"),
                "Finish taxes",
                null,
                false,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null)
        };

        using var ctx = CreateContext(seed);
        var cut = ctx.RenderComponent<TodoDashboard>();
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-test='todo-row']")));

        cut.Find("[data-test='toggle-button']").Click();

        cut.WaitForAssertion(() =>
        {
            var row = cut.Find("[data-test='todo-row']");
            Assert.Contains("is-completed", row.ClassList);
        });
    }

    [Fact]
    public void DeleteFlow_RemovesRowAndShowsEmptyState()
    {
        var seed = new[]
        {
            TodoItem.Rehydrate(
                Guid.Parse("cd3477c5-97cf-4dcc-88cc-44c06cec9bcb"),
                "Archive docs",
                null,
                false,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                null)
        };

        using var ctx = CreateContext(seed);
        var cut = ctx.RenderComponent<TodoDashboard>();
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-test='todo-row']")));

        cut.Find("[data-test='delete-button']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll("[data-test='todo-row']"));
            var empty = cut.Find("[data-test='empty-state']");
            Assert.Contains("add your first todo", empty.TextContent, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static TestContext CreateContext(IEnumerable<TodoItem>? seed = null)
    {
        var ctx = new TestContext();
        ctx.Services.AddLogging();
        ctx.Services.AddScoped<ITodoRepository>(_ => new InMemoryTodoRepository(seed));
        ctx.Services.AddScoped<TodoStateService>();
        return ctx;
    }

    private sealed class InMemoryTodoRepository : ITodoRepository
    {
        private readonly List<TodoItem> items;
        private TodoFilter filter = TodoFilter.Default;

        public InMemoryTodoRepository(IEnumerable<TodoItem>? seed = null)
        {
            items = seed?.Select(Clone).ToList() ?? new List<TodoItem>();
        }

        public Task<IReadOnlyList<TodoItem>> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TodoItem>>(items.Select(Clone).ToList());

        public Task SaveAsync(IReadOnlyList<TodoItem> payload, CancellationToken cancellationToken = default)
        {
            items.Clear();
            items.AddRange(payload.Select(Clone));
            return Task.CompletedTask;
        }

        public Task<TodoFilter> LoadFilterAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(filter);

        public Task SaveFilterAsync(TodoFilter selection, CancellationToken cancellationToken = default)
        {
            filter = selection;
            return Task.CompletedTask;
        }

        private static TodoItem Clone(TodoItem source) =>
            TodoItem.Rehydrate(
                source.Id,
                source.Title,
                source.Note,
                source.IsCompleted,
                source.CreatedAt,
                source.UpdatedAt,
                source.DueDay);
    }
}
