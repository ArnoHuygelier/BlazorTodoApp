using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using BlazorTodoApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorTodoApp.Services;

public sealed class LocalStorageTodoRepository : ITodoRepository
{
    internal const string ItemsStorageKey = "todoItems.v1";
    internal const string FilterStorageKey = "todoFilter.v1";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IJSRuntime jsRuntime;
    private readonly ILogger<LocalStorageTodoRepository> logger;

    public LocalStorageTodoRepository(IJSRuntime jsRuntime, ILogger<LocalStorageTodoRepository> logger)
    {
        this.jsRuntime = jsRuntime;
        this.logger = logger;
    }

    public async Task<IReadOnlyList<TodoItem>> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await jsRuntime.InvokeAsync<string?>("todoStorage.get", cancellationToken, ItemsStorageKey);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return Array.Empty<TodoItem>();
            }

            var dtos = JsonSerializer.Deserialize<List<TodoItemDto>>(payload, SerializerOptions);

            if (dtos is null || dtos.Count == 0)
            {
                return Array.Empty<TodoItem>();
            }

            return dtos
                .Select(dto => TodoItem.Rehydrate(
                    dto.Id,
                    dto.Title ?? string.Empty,
                    dto.Note,
                    dto.IsCompleted,
                    dto.CreatedAt,
                    dto.UpdatedAt,
                    dto.DueDay))
                .ToList();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Unable to parse todo data from {StorageKey}. Resetting payload.", ItemsStorageKey);
            await RemoveKeyAsync(ItemsStorageKey, cancellationToken);
            return Array.Empty<TodoItem>();
        }
        catch (JSException ex)
        {
            logger.LogError(ex, "JS interop failed while loading todos.");
            return Array.Empty<TodoItem>();
        }
    }

    public async Task SaveAsync(IReadOnlyList<TodoItem> items, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = items.Select(TodoItemDto.FromModel).ToList();
            var payload = JsonSerializer.Serialize(dto, SerializerOptions);
            await jsRuntime.InvokeAsync<object?>("todoStorage.set", cancellationToken, ItemsStorageKey, payload);
        }
        catch (JSException ex)
        {
            logger.LogError(ex, "JS interop failed while saving todos.");
            throw;
        }
    }

    public async Task<TodoFilter> LoadFilterAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await jsRuntime.InvokeAsync<string?>("todoStorage.get", cancellationToken, FilterStorageKey);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return TodoFilter.Default;
            }

            var dto = JsonSerializer.Deserialize<TodoFilterDto>(payload, SerializerOptions);

            if (dto?.Selection is string selectionValue &&
                Enum.TryParse<TodoFilterOption>(selectionValue, ignoreCase: true, out var parsedSelection) &&
                TodoFilter.IsValid(parsedSelection))
            {
                return TodoFilter.FromSelection(parsedSelection);
            }

            await SaveFilterAsync(TodoFilter.Default, cancellationToken);
            return TodoFilter.Default;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid filter payload detected. Resetting to default.");
            await SaveFilterAsync(TodoFilter.Default, cancellationToken);
            return TodoFilter.Default;
        }
        catch (JSException ex)
        {
            logger.LogError(ex, "JS interop failed while loading todo filter.");
            return TodoFilter.Default;
        }
    }

    public async Task SaveFilterAsync(TodoFilter filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new TodoFilterDto(filter.Selection.ToString());
            var payload = JsonSerializer.Serialize(dto, SerializerOptions);
            await jsRuntime.InvokeAsync<object?>("todoStorage.set", cancellationToken, FilterStorageKey, payload);
        }
        catch (JSException ex)
        {
            logger.LogError(ex, "JS interop failed while saving todo filter.");
            throw;
        }
    }

    private async ValueTask RemoveKeyAsync(string key, CancellationToken cancellationToken)
    {
        await jsRuntime.InvokeAsync<object?>("todoStorage.remove", cancellationToken, key);
    }

    private sealed record TodoItemDto(
        Guid Id,
        string? Title,
        string? Note,
        bool IsCompleted,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateOnly? DueDay)
    {
        public static TodoItemDto FromModel(TodoItem item) =>
            new(
                item.Id,
                item.Title,
                item.Note,
                item.IsCompleted,
                item.CreatedAt,
                item.UpdatedAt,
                item.DueDay);
    }

    private sealed record TodoFilterDto(string Selection);
}
