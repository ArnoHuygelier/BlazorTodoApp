using System.Text.Json;
using BlazorTodoApp.Models;
using BlazorTodoApp.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;

namespace BlazorTodoApp.Tests.Services;

public class LocalStorageTodoRepositoryTests
{
    private readonly Mock<IJSRuntime> jsRuntime = new();
    private readonly LocalStorageTodoRepository repository;

    public LocalStorageTodoRepositoryTests()
    {
        repository = new LocalStorageTodoRepository(jsRuntime.Object, NullLogger<LocalStorageTodoRepository>.Instance);
    }

    [Fact]
    public async Task LoadAsync_ReturnsItems_WhenPayloadValid()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2025, 2, 10, 9, 30, 0, TimeSpan.Zero);
        var payload =
            $$"""
              [
                  {
                      "id": "{{id}}",
                      "title": "Write tests",
                      "note": "Include serialization coverage",
                      "isCompleted": true,
                      "createdAt": "{{createdAt:O}}",
                      "updatedAt": "{{createdAt:O}}",
                      "dueDay": "2025-02-15"
                  }
              ]
              """;

        SetupGet(LocalStorageTodoRepository.ItemsStorageKey, payload);

        var result = await repository.LoadAsync();

        var item = Assert.Single(result);
        Assert.Equal(id, item.Id);
        Assert.Equal("Write tests", item.Title);
        Assert.Equal("Include serialization coverage", item.Note);
        Assert.True(item.IsCompleted);
        Assert.Equal(new DateOnly(2025, 2, 15), item.DueDay);
    }

    [Fact]
    public async Task SaveAsync_PersistsNormalizedPayload()
    {
        string? capturedKey = null;
        string? capturedPayload = null;
        SetupSet((key, payload) =>
        {
            capturedKey = key;
            capturedPayload = payload;
        });

        var createdAt = new DateTimeOffset(2025, 2, 12, 12, 0, 0, TimeSpan.Zero);
        var item = TodoItem.Rehydrate(
            Guid.Parse("4f6ddc55-eda3-4188-87e2-0f90f1a6d68f"),
            "Plan sprint",
            null,
            false,
            createdAt,
            createdAt,
            null);

        await repository.SaveAsync(new[] { item });

        Assert.Equal(LocalStorageTodoRepository.ItemsStorageKey, capturedKey);
        Assert.NotNull(capturedPayload);

        using var doc = JsonDocument.Parse(capturedPayload!);
        var element = Assert.Single(doc.RootElement.EnumerateArray());
        Assert.Equal("plan sprint", element.GetProperty("title").GetString()?.ToLowerInvariant());
        Assert.False(element.GetProperty("isCompleted").GetBoolean());
        Assert.False(element.TryGetProperty("note", out _), "Note should be omitted when null.");
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_ClearsStorageAndReturnsEmpty()
    {
        SetupGet(LocalStorageTodoRepository.ItemsStorageKey, "{]");
        string? removedKey = null;
        SetupRemove(key => removedKey = key);

        var result = await repository.LoadAsync();

        Assert.Empty(result);
        Assert.Equal(LocalStorageTodoRepository.ItemsStorageKey, removedKey);
    }

    [Fact]
    public async Task LoadFilterAsync_InvalidSelection_ResetsToDefault()
    {
        SetupGet(LocalStorageTodoRepository.FilterStorageKey, """{ "selection": "banana" }""");
        string? savedKey = null;
        string? savedPayload = null;
        SetupSet((key, payload) =>
        {
            if (key == LocalStorageTodoRepository.FilterStorageKey)
            {
                savedKey = key;
                savedPayload = payload;
            }
        });

        var filter = await repository.LoadFilterAsync();

        Assert.Equal(TodoFilterOption.All, filter.Selection);
        Assert.Equal(LocalStorageTodoRepository.FilterStorageKey, savedKey);
        Assert.NotNull(savedPayload);

        using var doc = JsonDocument.Parse(savedPayload!);
        Assert.Equal("All", doc.RootElement.GetProperty("selection").GetString());
    }

    private void SetupGet(string expectedKey, string? payload)
    {
        jsRuntime.Setup(runtime => runtime.InvokeAsync<string?>(
                "todoStorage.get",
                It.IsAny<CancellationToken>(),
                It.Is<object?[]?>(args => MatchesKey(args, expectedKey))))
            .Returns(new ValueTask<string?>(payload));
    }

    private void SetupSet(Action<string, string> callback)
    {
        jsRuntime.Setup(runtime => runtime.InvokeAsync<object?>(
                "todoStorage.set",
                It.IsAny<CancellationToken>(),
                It.IsAny<object?[]?>()))
            .Returns((string _, CancellationToken _, object?[]? args) =>
            {
                if (args is { Length: 2 } &&
                    args[0] is string key &&
                    args[1] is string payload)
                {
                    callback(key, payload);
                }

                return new ValueTask<object?>(result: null);
            });
    }

    private void SetupRemove(Action<string> callback)
    {
        jsRuntime.Setup(runtime => runtime.InvokeAsync<object?>(
                "todoStorage.remove",
                It.IsAny<CancellationToken>(),
                It.Is<object?[]?>(args => MatchesKey(args, LocalStorageTodoRepository.ItemsStorageKey))))
            .Returns((string _, CancellationToken _, object?[]? args) =>
            {
                if (args is { Length: 1 } && args[0] is string key)
                {
                    callback(key);
                }

                return new ValueTask<object?>(result: null);
            });
    }

    private static bool MatchesKey(object?[]? args, string expected) =>
        args is { Length: 1 } &&
        args[0] is string supplied &&
        supplied == expected;
}
