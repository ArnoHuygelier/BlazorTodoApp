using BlazorTodoApp.Models;

namespace BlazorTodoApp.Services;

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(IReadOnlyList<TodoItem> items, CancellationToken cancellationToken = default);

    Task<TodoFilter> LoadFilterAsync(CancellationToken cancellationToken = default);

    Task SaveFilterAsync(TodoFilter filter, CancellationToken cancellationToken = default);
}
