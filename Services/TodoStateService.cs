using System.Linq;
using BlazorTodoApp.Models;
using Microsoft.Extensions.Logging;

namespace BlazorTodoApp.Services;

public sealed class TodoStateService
{
    private readonly ITodoRepository repository;
    private readonly ILogger<TodoStateService> logger;
    private readonly List<TodoItem> items = new();

    private TodoFilter currentFilter = TodoFilter.Default;
    private DashboardSummary summary = DashboardSummary.Empty;

    public event EventHandler? StateChanged;

    public bool IsInitialized { get; private set; }

    public IReadOnlyList<TodoItem> Items => items.AsReadOnly();

    public TodoFilter CurrentFilter => currentFilter;

    public DashboardSummary Summary => summary;

    public TodoStateService(ITodoRepository repository, ILogger<TodoStateService> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
        {
            return;
        }

        logger.LogInformation("Initializing todo state service.");

        var loadedItems = await repository.LoadAsync(cancellationToken);
        items.Clear();
        items.AddRange(loadedItems.OrderBy(item => item.CreatedAt));

        currentFilter = await repository.LoadFilterAsync(cancellationToken);
        RecalculateSummary();
        IsInitialized = true;

        logger.LogInformation("Initialization complete with {TodoCount} todos and filter {FilterSelection}.", items.Count, currentFilter.Selection);
        NotifyStateChanged();
    }

    public IReadOnlyList<TodoItem> GetFilteredItems() =>
        currentFilter.Selection switch
        {
            TodoFilterOption.Active => items.Where(item => !item.IsCompleted).ToList(),
            TodoFilterOption.Completed => items.Where(item => item.IsCompleted).ToList(),
            _ => items.ToList()
        };

    public async Task<TodoItem> AddTodoAsync(string title, string? note, DateOnly? dueDay, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var timestamp = DateTimeOffset.UtcNow;
        var newItem = TodoItem.Create(title, note, dueDay, timestamp);
        EnsureUniqueTitle(newItem.GetNormalizedTitleKey(), null);
        items.Add(newItem);

        await PersistAsync(cancellationToken);
        RecalculateSummary();
        logger.LogInformation("Added todo {TodoId} with title {Title}.", newItem.Id, newItem.Title);
        NotifyStateChanged();
        return newItem;
    }

    public async Task UpdateTodoAsync(Guid id, string title, string? note, DateOnly? dueDay, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var existing = GetExisting(id);
        EnsureUniqueTitle(TodoItem.BuildNormalizedTitleKey(title), id);

        existing.UpdateDetails(title, note, dueDay, DateTimeOffset.UtcNow);
        await PersistAsync(cancellationToken);
        RecalculateSummary();
        logger.LogInformation("Updated todo {TodoId} with title {Title}.", existing.Id, existing.Title);
        NotifyStateChanged();
    }

    public async Task ToggleTodoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var existing = GetExisting(id);
        existing.SetCompletion(!existing.IsCompleted, DateTimeOffset.UtcNow);

        await PersistAsync(cancellationToken);
        RecalculateSummary();
        logger.LogInformation("Toggled todo {TodoId} to completed={Completed}.", existing.Id, existing.IsCompleted);
        NotifyStateChanged();
    }

    public async Task DeleteTodoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var removed = items.RemoveAll(item => item.Id == id) > 0;

        if (!removed)
        {
            throw new InvalidOperationException($"Todo with id {id} was not found.");
        }

        await PersistAsync(cancellationToken);
        RecalculateSummary();
        logger.LogInformation("Deleted todo {TodoId}. Remaining count {Count}.", id, items.Count);
        NotifyStateChanged();
    }

    public async Task SetFilterAsync(TodoFilterOption selection, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        if (currentFilter.Selection == selection)
        {
            return;
        }

        currentFilter = currentFilter.WithSelection(selection);
        await repository.SaveFilterAsync(currentFilter, cancellationToken);
        RecalculateSummary();
        logger.LogInformation("Changed todo filter to {FilterSelection}.", currentFilter.Selection);
        NotifyStateChanged();
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (!IsInitialized)
        {
            await InitializeAsync(cancellationToken);
        }
    }

    private void EnsureUniqueTitle(string normalizedTitle, Guid? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new ArgumentException("Title is required.", nameof(normalizedTitle));
        }

        var duplicate = items.Any(item => item.GetNormalizedTitleKey() == normalizedTitle && item.Id != ignoreId);

        if (duplicate)
        {
            logger.LogWarning("Duplicate todo title attempt detected for normalized key {NormalizedTitle}.", normalizedTitle);
            throw new InvalidOperationException("A todo with the same title already exists.");
        }
    }

    private TodoItem GetExisting(Guid id)
    {
        var todo = items.FirstOrDefault(item => item.Id == id);

        if (todo is null)
        {
            throw new InvalidOperationException($"Todo with id {id} was not found.");
        }

        return todo;
    }

    private Task PersistAsync(CancellationToken cancellationToken) =>
        repository.SaveAsync(items, cancellationToken);

    private void RecalculateSummary()
    {
        var total = items.Count;
        var active = items.Count(item => !item.IsCompleted);
        var completed = total - active;
        summary = new DashboardSummary(total, active, completed, currentFilter.Selection);
    }

    private void NotifyStateChanged()
    {
        try
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TodoStateService state change notification failed.");
        }
    }
}
