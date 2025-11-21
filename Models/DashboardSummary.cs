namespace BlazorTodoApp.Models;

public sealed record DashboardSummary(int Total, int Active, int Completed, TodoFilterOption Filter)
{
    public static DashboardSummary Empty { get; } = new(0, 0, 0, TodoFilterOption.All);
}
